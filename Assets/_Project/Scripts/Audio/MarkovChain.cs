using System.Collections.Generic;
using UnityEngine;

namespace Arekntt.Core
{
    public class MarkovChain<T>
    {
        // transitions[from] = lista (to, prawdopodobieñstwo)
        private Dictionary<T, List<(T next, float weight)>> transitions = new();
        private T current;

        public MarkovChain(T initial)
        {
            current = initial;
        }

        public void AddTransition(T from, T to, float weight)
        {
            if (!transitions.ContainsKey(from))
                transitions[from] = new List<(T, float)>();

            transitions[from].Add((to, weight));
        }

        public T Next()
        {
            if (!transitions.ContainsKey(current))
                return current;

            var options = transitions[current];

            // suma wag
            float total = 0f;
            foreach (var o in options) total += o.weight;

            float roll = Random.Range(0f, total);
            float cumulative = 0f;

            foreach (var o in options)
            {
                cumulative += o.weight;
                if (roll <= cumulative)
                {
                    current = o.next;
                    return current;
                }
            }

            current = options[options.Count - 1].next;
            return current;
        }

        public T Current => current;
    }
}
