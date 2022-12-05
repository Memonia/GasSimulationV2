#include <thread>
#include <stop_token>
#include <mutex>

#include "collisions.cuh"
#include "simulation.h"

Particle* deep_copy(const Particle* particles, int count)
{
	auto* copy = new Particle[count];
	for (size_t i = 0; i < count; ++i)
		copy[i] = particles[i];
	return copy;
}

Simulator::Simulator(
	const Collidables& collidables, 
	const SimulatorInfoOut& infoOut,
	const SimulationStateOut& stateOut,
	size_t queueSize) : _queueSize{ queueSize }
{
	_simulatorInfo = infoOut;

	_currentCommitedState.Particles = stateOut.Particles;
	_currentCommitedState.Collisions = stateOut.Collisions;

	// Never modified
	_collidables.Surfaces = collidables.Surfaces;
	_collidables.SurfaceCount = collidables.SurfaceCount;

	// This is acted upon, so initially we make a copy
	_collidables.Particles = deep_copy(
		collidables.Particles, collidables.ParticleCount);
	_collidables.ParticleCount = collidables.ParticleCount;
}

Simulator::~Simulator()
{
	delete[] _collidables.Particles;
	
	SimulationState out;
	while (_commitQueue.try_pop(out)) 
		delete[] out.Particles;
}

void Simulator::_simulate(std::stop_token token)
{
	int collisions = 0;
	double frameTime = 1;
	while (frameTime > 0)
	{
		if (token.stop_requested())
			return;

		auto ci{ detectNearestAndAdvanceCuda(_collidables, frameTime) };
		if (ci.Time < frameTime)
		{
			// Resolution (see CollisionInfo)
			if (ci.CollidableIndex < _collidables.ParticleCount)
			{
				resolveCollision(
					_collidables.Particles[ci.ParticleIndex],
					_collidables.Particles[ci.CollidableIndex]
				);
			}

			else
			{
				resolveCollision(
					_collidables.Particles[ci.ParticleIndex],
					_collidables.Surfaces[
						ci.CollidableIndex - _collidables.ParticleCount
					]
				);
			}

			++collisions;
			frameTime -= ci.Time;
		}

		else
			frameTime = 0;
	}

	if (token.stop_requested())
		return;

	SimulationState state;
	state.Collisions = collisions;
	state.Particles = deep_copy(	
		_collidables.Particles, _collidables.ParticleCount
	);

	_commitQueue.push(state);
	*_simulatorInfo.QueueLength += 1;
}

bool Simulator::Next()
{
	SimulationState newState;
	if (_commitQueue.try_pop(newState))
	{
		_queueCondVar.notify_all();

		*_simulatorInfo.QueueLength -= 1;
		*_currentCommitedState.Collisions = newState.Collisions;
		for (size_t i = 0; i < _collidables.ParticleCount; ++i)
		{
			_currentCommitedState.Particles[i]
				.Center = newState.Particles[i].Center;

			_currentCommitedState.Particles[i]
				.Velocity = newState.Particles[i].Velocity;
		}

		delete[] newState.Particles;
		return true;
	}

	return false;
}

void Simulator::Step()
{
	_simulate(std::stop_token());
}

void Simulator::StartSimulation()
{
	_simulationThread = std::jthread([this](std::stop_token token)
	{
		while (true)
		{
			if (token.stop_requested())
				break;

			_simulate(token);
			if (_commitQueue.unsafe_size() >= _queueSize)
			{
				std::unique_lock lk(_queueMutex);
				_queueCondVar.wait(lk);
			}
		}
	});
}

void Simulator::StopSimulation()
{
	_simulationThread.request_stop();
	_queueCondVar.notify_all();
}

void Simulator::SpinJoin()
{
	if (_simulationThread.joinable())
		_simulationThread.join();
}
