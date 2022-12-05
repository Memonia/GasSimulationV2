using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;

using GasSimulationV2.Core.Interface;
using GasSimulationV2.GUI.DrawingLogic.VisualObjects;
using GasSimulationV2.Utils.Math;

using Timer = System.Timers.Timer;
using WindowsPoint = System.Windows.Point;
using WindowsVector = System.Windows.Vector;

namespace GasSimulationV2.GUI.DrawingLogic.Updaters
{
    internal partial class CanvasUpdater
    {
		public bool TrackAll { get; set; }
		public bool HideNotTracked { get; set; }
		public bool ShowVelocityVector { get; set; }
		public bool ShowTrajectoryLine { get; set; }
		public bool ShowSlowest { get; set; }
		public bool ShowFastest { get; set; }
		public bool UpdateParticleContainer { get; set; }

        private bool _lockedTrackAll;
		private bool _lockedHideNotTracked;
		private bool _lockedShowVelocityVector;
        private bool _lockedShowTrajectoryLine;
        private bool _lockedShowSlowest;
        private bool _lockedShowFastest;

		public InfoBlockUpdater? InfoBlockUpdater { get; set; }

        private bool _isUpdating = false;
        private bool _initialised = false;

        private Timer _updateTimer;
        
        private readonly ISimulator _simulator;
		private readonly SelfUpdatingCanvas _canvas;

		// Updater works outside of UI thread, so it can't create visual
		// elemnets directly, so we pass the info needed to create those
		// elements to canvas
		private readonly List<VisualParticle> _visualParticles;

        public CanvasUpdater(SelfUpdatingCanvas canvas, ISimulator simulator)
        {
			_updateTimer = new();
			_simulator = simulator;
			_canvas = canvas;
            _visualParticles = new();
		}

        public void Init()
        {
            _visualParticles.Clear();
            foreach (var p in _simulator.CurrentFrame.Particles)
                _visualParticles.Add(new VisualParticle(p));

            _canvas.Init(_visualParticles);
            _canvas.Update(_visualParticles, _getLockedSettings());
            _initialised = true;
        }

        public void StartUpdating(double updateInterval, CancellationToken token)
        {
            void _updateCallback(object? o, ElapsedEventArgs e)
            {
                if (token.IsCancellationRequested)
                {
                    _updateTimer.Stop();
                    _updateTimer.Close();
                    return;
                }

                _update(token);
            }

            if (!_initialised)
                throw new InvalidOperationException(
                    "Can't start updating before 'Init' is called"
                );

            if (_updateTimer.Enabled)
                throw new InvalidOperationException(
                    "Updating has been already started"
                );

            _updateTimer.Close();
            _updateTimer = new(updateInterval);
            _updateTimer.Elapsed += _updateCallback;
            _updateTimer.AutoReset = true;
            _updateTimer.Start();
        }

        private void _lockSettings()
        {
            _lockedTrackAll = TrackAll;
            _lockedHideNotTracked = HideNotTracked;
            _lockedShowVelocityVector= ShowVelocityVector;
			_lockedShowTrajectoryLine = ShowTrajectoryLine;
            _lockedShowSlowest = ShowSlowest;
            _lockedShowFastest = ShowFastest;
		}

        private VisualisationSettings _getLockedSettings()
        {
            return new(
                _lockedTrackAll, 
                _lockedHideNotTracked,
                _lockedShowVelocityVector, 
                _lockedShowTrajectoryLine,
                _lockedShowSlowest,
                _lockedShowFastest
            );
        }

        private void _update(CancellationToken token)
        {
            if (_isUpdating)
                return;

			_isUpdating = true;

			// The settings might be changed in the middle of the
            // long-running calculation.
			// When updated particles are passed to the canvas to be drawn,
			// the canvas might not expect certain objects to not be there,
			// when settings suggest otherwise.
			//
			// To avoid any weird effects, the settings are locked and
            // are passed to the canvas explicitly
			_lockSettings();

            if (UpdateParticleContainer)
                _simulator.Next();

			var min = _visualParticles[0];
			var max = _visualParticles[0];
			for (int i = 0; i < _visualParticles.Count; ++i)
            {
                if (token.IsCancellationRequested)
                    return;

				var vp = _visualParticles[i];
                vp.IsSlowest = false;
                vp.IsFastest = false;

				if (min.Particle.Velocity.Length > vp.Particle.Velocity.Length)
					min = vp;

				if (max.Particle.Velocity.Length < vp.Particle.Velocity.Length)
					max = vp;
		
                if (vp.IsTracked || _lockedTrackAll)
				{
					if (_lockedShowTrajectoryLine)
						_updateTrajectoryLine(vp);

					if (_lockedShowVelocityVector)
                        _updateVelocityVector(vp);
                }
            }

            if (_lockedShowSlowest)
            {
				min.IsSlowest = true;
				if (_lockedShowTrajectoryLine) _updateTrajectoryLine(min);
				if (_lockedShowVelocityVector) _updateVelocityVector(min);
			}            
            
            if (_lockedShowFastest)
			{
				max.IsFastest = true;
				if (_lockedShowTrajectoryLine) _updateTrajectoryLine(max);
				if (_lockedShowVelocityVector) _updateVelocityVector(max);
			}

			// UI thread is activated only after everything has been precalculated
			_canvas.Dispatcher.Invoke(() =>
                _canvas.Update(_visualParticles, _getLockedSettings())
            );

            InfoBlockUpdater?.Update(
				_simulator.CurrentFrame.Collisions,
				_simulator.QueueLength
            );

            _isUpdating = false;
        }

        private void _updateVelocityVector(VisualParticle vp)
        {
            var p = vp.Particle;
            var normal = p.Velocity.Normal();

            WindowsVector velocity = new(p.Velocity.Vx, p.Velocity.Vy);
            WindowsVector velocityNormal = new(normal.Vx, normal.Vy);

            velocity.Normalize();
            velocityNormal.Normalize();

            // Arrow tip height and width
            double h = p.R / 10;
            double w = p.R / 10;

            WindowsPoint p1 = new(p.Center.Vx, p.Center.Vy);
            WindowsPoint p2 = p1 + velocity * (p.R + p.Velocity.Length);
            WindowsPoint p4 = p2;
            WindowsPoint p6 = p2;

            WindowsPoint O = p2 - velocity * h;
            WindowsPoint p3 = O + velocityNormal * w;
            WindowsPoint p5 = O + velocityNormal * -w;

            vp.Velocity.SetPoints(
                new WindowsPoint[] { p1, p2, p3, p4, p5, p6 }
            );
		}

        private void _updateTrajectoryLine(VisualParticle vp)
        {
            var lineType = vp.Particle.TrajectoryLine.Type;
            if (lineType == StraightLineType.NotLine)
                return;

            double x1, x2, y1, y2;
            if (lineType == StraightLineType.Horizontal)
            {
                x1 = 0;
                x2 = _canvas.ActualWidth;
                y1 = vp.Particle.TrajectoryLine.GetY(x1);
                y2 = vp.Particle.TrajectoryLine.GetY(x2);
            }

            // Vertical or arbitrary line
            else
            {
                y1 = 0;
                y2 = _canvas.ActualHeight;
                x1 = vp.Particle.TrajectoryLine.GetX(y1);
                x2 = vp.Particle.TrajectoryLine.GetX(y2);
            }

            vp.Trajectory.P1 = new(x1, y1);
            vp.Trajectory.P2 = new(x2, y2);
        }
    }
}
