using System.Collections.Generic;

namespace Arekntt.Core
{
    public class EventQueue
    {
        private List<object> current = new();
        private List<object> next = new();

        public void Enqueue(object e) => next.Add(e);

        // NOWE: widoczne w tej samej klatce EnqueueImmediate dla krytycznych eventów
        public void EnqueueImmediate(object e) => current.Add(e);

        // ZMIANA: zwraca kopiê — bezpieczna iteracja
        public List<object> GetEvents() => new List<object>(current);

        public void Swap()
        {
            current = new List<object>(next);
            next.Clear();
        }
    }
}
