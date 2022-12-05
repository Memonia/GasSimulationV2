#ifndef SIMULATION_H
#define SIMULATION_H

#include <concurrent_queue.h>
#include <stop_token>
#include <mutex>
#include <condition_variable>
#include <vector>
#include <thread>

#include "structures.cuh"

struct SimulationStateOut
{
public:
	int* Collisions;
	Particle* Particles;
};

struct SimulatorInfoOut
{
public:
	int* QueueLength;
};

struct SimulationState
{
public:
	int Collisions;
	Particle* Particles;
};

class Simulator
{
private:
	const size_t _queueSize;
	concurrency::concurrent_queue<SimulationState> _commitQueue;

	std::jthread _simulationThread;
	std::mutex _queueMutex;
	std::condition_variable _queueCondVar;

	Collidables _collidables;
	SimulatorInfoOut _simulatorInfo;
	SimulationStateOut _currentCommitedState;

	void _simulate(std::stop_token token);

public:
	Simulator(
		const Collidables& collidables, 
		const SimulatorInfoOut& infoOut,
		const SimulationStateOut& stateOut,
		size_t queueSize = 2048
	);

	~Simulator();

	bool Next();
	void Step();
	void StartSimulation();
	void StopSimulation();
	void SpinJoin();
};

#endif // !SIMULATION_H
