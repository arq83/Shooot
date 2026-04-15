using System.Collections.Generic;

namespace Arekntt.Core
{
    public class GameLoop
    {
        private List<ISystem> systems;
        private EventQueue queue;

        public GameLoop(List<ISystem> systems, EventQueue queue)
        {
            this.systems = systems;
            this.queue = queue;
        }

        public void Update()
        {
            queue.Swap();

            foreach (var system in systems)
            {
                system.Update();
            }
        }
    }
}
