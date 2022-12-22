using System.Collections.Concurrent;

using GasSimulationV2.Core.Collisions;
using GasSimulationV2.Core.Extensions;
using GasSimulationV2.Core.Interface;

using static GasSimulationV2.Core.Collisions.CollisionResolution;

namespace GasSimulationV2.Core
{
	internal class Simulator : ISimulator
	{
		// Used to suspend simulating thread in case the commit queue is full
		private readonly ManualResetEvent _waitHandle;

		// Allows the simulation to keep running,
		// without waiting for consumers to move the simulation forward
		private readonly ConcurrentQueue<SimulationState> _commitQueue;

		private readonly int _queueSize;
		private SimulationState _currentCommitedState;

		private readonly Container _container;
		private readonly CollisionDetector _detector;

		private bool _wasReset = true;
		private bool _isRunning = false;
		private bool _forceStop = false;
		private bool _disposedValue = false;

		public int QueueLength => _commitQueue.Count;

		public Frame CurrentFrame =>
			new
			(
				_currentCommitedState.Collisions, 
				_currentCommitedState.Particles
			);

		public Simulator(Container container, int queueSize = 256)
		{
			_waitHandle = new(true);	
			_detector = new();
			_commitQueue = new();
			_container = container;
			_queueSize = queueSize;
		}

		private void _waitForSpace() => _waitHandle.WaitOne();

		private void _pauseUpdating() => _waitHandle.Reset();

		private void _resumeUpdating() => _waitHandle.Set();

		private void _simulate(CancellationToken token)
		{
			int collisions = 0;
			double frameTime = 1;
			while (frameTime > 0)
			{
				if (token.IsCancellationRequested)
					return;

				CollisionInfo ci = _detector.NearestCollision(
					_container.Collidables, token);

				// Checking if the nearest collision is within the remaining frame
				if (ci.Time <= frameTime)
				{
					foreach (var p in _container.Collidables.Particles)
						p.Center += p.Velocity * ci.Time;

					ResolveCollision(ci.Particle, ci.Collidable);

					++collisions;
					frameTime -= ci.Time;
				}

				else
				{
					foreach (var p in _container.Collidables.Particles)
						p.Center += p.Velocity * frameTime;

					frameTime = 0;
				}
			}

			if (token.IsCancellationRequested)
				return;

			_commitQueue.Enqueue(
				new SimulationState()
				{
					Collisions = collisions,
					Particles = _container.Collidables.Particles.DeepCopy()
				}
			);
		}

		public void Step()
		{
			if (_isRunning)
				throw new InvalidOperationException("Simulation is still running");

			_simulate(new());
		}

		public void StartSimulation(CancellationToken token)
		{
			void _threadBody()
			{
				while (!_forceStop && !token.IsCancellationRequested)
				{
					if (_commitQueue.Count >= _queueSize)
						_pauseUpdating();

					_waitForSpace();
					_simulate(token);
				}

				_isRunning = false;
			}

			if (_isRunning)
				throw new InvalidOperationException(
					"Simulation has been already started"
				);

			// Simulating thread might be blocked on waiting for free space
			token.Register(_resumeUpdating);

			_isRunning = true;
			new Thread(_threadBody).Start();
		}

		public bool Next()
		{
			if (_commitQueue.TryDequeue(out SimulationState newState))
			{
				_resumeUpdating();
				_currentCommitedState
					.Collisions = newState.Collisions;

				for (int i = 0; i < newState.Particles.Count; ++i)
				{
					_currentCommitedState.Particles[i].Center = 
						newState.Particles[i].Center;

					_currentCommitedState.Particles[i].Velocity = 
						newState.Particles[i].Velocity;
				}

				return true;
			}

			return false;
		}

		public void Reset()
		{
			if (_isRunning)
				throw new InvalidOperationException("Simulation is still running");

			_container.Clear();
			_commitQueue.Clear();

			_wasReset = true;
		}

		public void SpinJoin()
		{
			SpinWait.SpinUntil(() => !_isRunning);
		}

		public void SpawnParticles(SpawnParameters spawnParams, int? seed = null)
		{
			if (_isRunning)
				throw new InvalidOperationException("Simulation is still running");

			if (!_wasReset)
				throw new InvalidOperationException("Simulation was not reset");
			
			_container.Spawn(spawnParams, seed);
			_currentCommitedState = new()
			{
				Collisions = 0,
				Particles = _container.Collidables.Particles.DeepCopy()
			};

			_wasReset = false;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					_forceStop = true;
					_resumeUpdating();

					// Wait for simulation to finish to free resources
					SpinJoin();
					_waitHandle.Close();
				}

				_disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
