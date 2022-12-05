using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using GasSimulationV2.GUI.DrawingLogic.VisualObjects;

namespace GasSimulationV2.GUI.DrawingLogic
{
	internal partial class SelfUpdatingCanvas : Canvas
	{
		public SelfUpdatingCanvas()
		{
			this.ClipToBounds = true;
		}

		public void Init(List<VisualParticle> vpl)
		{
			this.Children.Clear();
			foreach (var vp in vpl)
			{
				this.Children.Add(vp);
				this.Children.Add(vp.Velocity);
				this.Children.Add(vp.Trajectory);
			}
		}

		public void Update(List<VisualParticle> vpl, VisualisationSettings stgs)
		{
			foreach (var vp in vpl)
			{
				// Won't be GC'ed.
				// The reference is still alive in 'VisualParticle'
				this.Children.Remove(vp.Velocity);
				this.Children.Remove(vp.Trajectory);

				if (vp.IsSlowest || vp.IsFastest || vp.IsTracked || 
					stgs.TrackAll)
				{
					if (stgs.ShowTrajectoryLine)
						_drawTrajectoryLine(vp);

					if (stgs.ShowVelocityVector)
						_drawVelocityVector(vp);
				}

				_updateTracked(vp, stgs);
				_drawVisualParticle(vp);
			}
		}

		private void _updateTracked(VisualParticle vp, VisualisationSettings stgs)
		{
			if (vp.IsSlowest) 
				vp.Fill = _slowestParticleColor;

			else 
			if (vp.IsFastest) 
				vp.Fill = _fastestParticleColor;

			else 
			if (stgs.HideNotTracked && !vp.IsTracked && !stgs.TrackAll)
				vp.Fill = _hiddenParticleColor;

			else 
			if (vp.IsTracked || stgs.TrackAll) 
				vp.Fill = _trackedParticleColor;

			else 
				vp.Fill = _particleColor;
		}

		private void _drawVisualParticle(VisualParticle vp)
		{
			Canvas.SetTop(vp, vp.Particle.Center.Vy);
			Canvas.SetLeft(vp, vp.Particle.Center.Vx);
		}

		private void _drawVelocityVector(VisualParticle vp)
		{
			vp.Velocity.StrokeThickness = _velocityVectorThickness;
			vp.Velocity.Stroke = _velocityVectorColor;
			this.Children.Add(vp.Velocity);
		}

		private void _drawTrajectoryLine(VisualParticle vp)
		{
			vp.Trajectory.StrokeThickness = _trajectoryLineThickness;
			vp.Trajectory.Stroke = _trajectoryLineColor;
			this.Children.Add(vp.Trajectory);
		}
	}
}
