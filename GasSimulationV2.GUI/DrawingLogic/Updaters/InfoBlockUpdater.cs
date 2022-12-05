using System.Windows.Controls;

namespace GasSimulationV2.GUI.DrawingLogic.Updaters
{
    internal class InfoBlockUpdater
    {
        private readonly TextBlock _infoBlock;

        public InfoBlockUpdater(TextBlock infoBlock)
        {
            _infoBlock = infoBlock;
        }

        public void Update(int collisions, int queueLength)
        {
            _infoBlock.Dispatcher.Invoke(() =>
            {
                _infoBlock.Text =
                    $"<Queue length: {queueLength:00000}; " +
                    $"Collisions: {collisions:000}>";
			});
        }
    }
}
