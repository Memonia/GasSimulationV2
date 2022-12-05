namespace GasSimulationV2.GUI.DrawingLogic
{
	internal record VisualisationSettings
	(
		bool TrackAll,
		bool HideNotTracked,
		bool ShowVelocityVector,
		bool ShowTrajectoryLine,
		bool ShowSlowest,
		bool ShowFastest
	);
}
