using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using GasSimulationV2.Core;
using GasSimulationV2.Core.Interface;
using GasSimulationV2.GUI.Actions;
using GasSimulationV2.GUI.DrawingLogic;
using GasSimulationV2.GUI.DrawingLogic.Updaters;

using CTS = System.Threading.CancellationTokenSource;
using UserAction = GasSimulationV2.GUI.Actions.Action;

namespace GasSimulationV2.GUI
{
	public partial class MainWindow : Window
	{
		private ISimulator _simulator;
		private CanvasUpdater _canvasUpdater;
		private InfoBlockUpdater _infoBlockUpdater;
		private SelfUpdatingCanvas _selfUpdatingCanvas;
		private readonly ActionStateMachine _actionStateMachine;

		// spf = 1/fps
		// multiply by 1000 so it's milliseconds and not seconds
		private const double _canvasUpdateInterval = 1d/144 * 1000;
		private UserAction _prevAction = UserAction.Start;

		private CTS _simulatorCTS = new();
		private readonly CTS _canvasUpdaterCTS = new();

		private readonly Regex _regexIntOnly = new(@"[0-9]");

		public MainWindow()
		{
			void _loadedCallback(object o, RoutedEventArgs e)
			{
				_simulator = SimulatorFactory.GetSimulator
				(
					CanvasContainer.ActualWidth,
					CanvasContainer.ActualHeight
				);

				_simulator.SpawnParticles(
					SpawnSettings.SpawnParameters, SpawnSettings.Seed
				);

				_selfUpdatingCanvas = new();

				_infoBlockUpdater = new(PerformanceInfoBlock);
				_canvasUpdater = new(_selfUpdatingCanvas, _simulator)
				{
					InfoBlockUpdater = _infoBlockUpdater
				};

				// Put canvas
				CanvasContainer.Child = _selfUpdatingCanvas;

				// Start updating immediately
				_canvasUpdater.Init();
				_canvasUpdater.StartUpdating(
					_canvasUpdateInterval, _canvasUpdaterCTS.Token
				);
			}

			InitializeComponent();

			// Show initial values
			SpeedBox.Text = SpawnSettings.SpeedSpan.ToString();
			SizesBox.Text = SpawnSettings.RadiusSpan.ToString();
			AmountBox.Text = SpawnSettings.ParticleAmount.ToString();
			SizeMultBox.Text = SpawnSettings.SizeMultiplyer.ToString();

			// The size is known when it fires
			this.Loaded += _loadedCallback;
			
			// Callbacks for each user action
			_actionStateMachine = new();
			_actionStateMachine.OnStart += _onStart;
			_actionStateMachine.OnStep += _onStep;
			_actionStateMachine.OnPause += _onPause;
			_actionStateMachine.OnResume += _onResume;
			_actionStateMachine.OnReset += _onReset;
		}

		private void _onStart()
		{
			_canvasUpdater.UpdateParticleContainer = true;
			_simulator.StartSimulation(_simulatorCTS.Token);

			StartOrResumeButton.Content = "Resume";
			StartOrResumeButton.IsEnabled = false;
			StepButton.IsEnabled = false;
			ResetButton.IsEnabled = false;
			PauseButton.IsEnabled = true;
			_prevAction = UserAction.Start;
		}
				
		private void _onStep()
		{
			_canvasUpdater.UpdateParticleContainer = true;
			_simulator.Step();

			StartOrResumeButton.IsEnabled = true;
			ResetButton.IsEnabled = true;
			PauseButton.IsEnabled = false;
			_prevAction = UserAction.Step;
		}

		private void _onPause()
		{
			_canvasUpdater.UpdateParticleContainer = false;
			
			StartOrResumeButton.Content = "Resume";
			StartOrResumeButton.IsEnabled = true;
			StepButton.IsEnabled = false;
			ResetButton.IsEnabled = true;
			PauseButton.IsEnabled = false;
			_prevAction = UserAction.Pause;
		}

		private void _onResume()
		{
			_canvasUpdater.UpdateParticleContainer = true;

			StartOrResumeButton.IsEnabled = false;
			StepButton.IsEnabled = false;
			ResetButton.IsEnabled = false;
			PauseButton.IsEnabled = true;
			_prevAction = UserAction.Resume;
		}

		private void _onReset()
		{
			_canvasUpdater.UpdateParticleContainer = false;
			_simulatorCTS.Cancel();
			_simulator.SpinJoin();

			_simulatorCTS = new();

			_simulator.Reset();
			_simulator.SpawnParticles(
				SpawnSettings.SpawnParameters, SpawnSettings.Seed
			);
			_canvasUpdater.Init();

			StartOrResumeButton.Content = "Start";
			StartOrResumeButton.IsEnabled = true;
			StepButton.IsEnabled = true;
			ResetButton.IsEnabled = true;
			PauseButton.IsEnabled = false;
			_prevAction = UserAction.Reset;
		}

		private void StartOrResume(object sender, RoutedEventArgs e)
		{
			switch (_prevAction)
			{
				case UserAction.Start:
				case UserAction.Step:
				case UserAction.Reset:
					_actionStateMachine.Switch(UserAction.Start);
					break;

				default:
					_actionStateMachine.Switch(UserAction.Resume);
					break;
			}
		}
			
		private void Step(object sender, RoutedEventArgs e)
		{
			_actionStateMachine.Switch(UserAction.Step);
		}

		private void Pause(object sender, RoutedEventArgs e)
		{
			_actionStateMachine.Switch(UserAction.Pause);
		}

		private void Reset(object sender, RoutedEventArgs e)
		{
			_actionStateMachine.Switch(UserAction.Reset);
		}

		private void SettingInput(object sender, TextCompositionEventArgs e)
		{
			if (!_regexIntOnly.IsMatch(e.Text))
				e.Handled = true;
		}

		private void SettingsChanged(object sender, TextChangedEventArgs e)
		{
			if (sender is not TextBox t)
				return;

			if (string.IsNullOrWhiteSpace(t.Text))
				t.Text = "0";

			if (t.Name == AmountBox.Name)
			{
				if (Int32.Parse(t.Text) > SpawnSettings.MaxParticleAmount)
					t.Text = SpawnSettings.MaxParticleAmount.ToString();
				if (Int32.Parse(t.Text) < SpawnSettings.MinParticleAmount)
					t.Text = SpawnSettings.MinParticleAmount.ToString();
				SpawnSettings.ParticleAmount = Int32.Parse(t.Text);
			}

			else
			if (t.Name == SpeedBox.Name)
			{
				if (Int32.Parse(t.Text) > SpawnSettings.MaxSpeedSpan)
					t.Text = SpawnSettings.MaxSpeedSpan.ToString();
				SpawnSettings.SpeedSpan = Int32.Parse(t.Text);
			}

			else
			if (t.Name == SizesBox.Name)
			{
				if (Int32.Parse(t.Text) > SpawnSettings.MaxRadiusSpan)
					t.Text = SpawnSettings.MaxRadiusSpan.ToString();
				SpawnSettings.RadiusSpan = Int32.Parse(t.Text);
			}

			else
			if (t.Name == SizeMultBox.Name)
			{
				if (Int32.Parse(t.Text) > SpawnSettings.MaxSizeMultiplyer)
					t.Text = SpawnSettings.MaxSizeMultiplyer.ToString();
				if (Int32.Parse(t.Text) < 1)
					t.Text = "1";
				SpawnSettings.SizeMultiplyer = Int32.Parse(t.Text);
			}

			t.CaretIndex = t.Text.Length;
		}

		private void TrackingInfoChanged(object sender, MouseButtonEventArgs e)
		{
			static void _checkDisabledAndSetColor(TextBlock t, bool @checked)
			{
				var resources = Application.Current.Resources;
				if (@checked) 
					t.Foreground = (Brush)resources["SelectedTextColor"];
				else t.Foreground = (Brush)resources["UnSelectedTextColor"];
			}

			if (sender is not TextBlock t)
				return;

			bool @checked = false;
			if (t.Name == TrajectoryCheck.Name)
			{
				@checked = !_canvasUpdater.ShowTrajectoryLine;
				_canvasUpdater.ShowTrajectoryLine = @checked;
			}

			else
			if (t.Name == VelocityCheck.Name)
			{
				@checked = !_canvasUpdater.ShowVelocityVector;
				_canvasUpdater.ShowVelocityVector = @checked;
			}		
			
			else
			if (t.Name == SlowestCheck.Name)
			{
				@checked = !_canvasUpdater.ShowSlowest;
				_canvasUpdater.ShowSlowest = @checked;
			}	
			
			else
			if (t.Name == FastestCheck.Name)
			{
				@checked = !_canvasUpdater.ShowFastest;
				_canvasUpdater.ShowFastest = @checked;
			}

			else
			if (t.Name == TrackAllCheck.Name)
			{
				@checked = !_canvasUpdater.TrackAll;
				_canvasUpdater.TrackAll = @checked;
			}

			else
			if (t.Name == HideCheck.Name)
			{
				@checked = !_canvasUpdater.HideNotTracked;
				_canvasUpdater.HideNotTracked = @checked;
			}

			else
			if (t.Name == BigParticleCheck.Name)
			{
				@checked = !SpawnSettings.BigParticle;
				SpawnSettings.BigParticle = @checked;
			}

			_checkDisabledAndSetColor(t, @checked);
		}

		private void OnClosing(object sender, CancelEventArgs e)
		{
			_simulatorCTS.Cancel();
			_canvasUpdaterCTS.Cancel();
		}
	}
}
