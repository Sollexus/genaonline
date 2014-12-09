using System;
using System.Collections.Generic;
using System.Linq;

namespace SollexMachineLearning {
	internal static class Helpers {
		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source) {
			var rng = new Random();
			var elements = source.ToArray();
			// Note i > 0 to avoid final pointless iteration
			for (var i = elements.Length - 1; i > 0; i--) {
				// Swap element "i" with a random earlier element it (or itself)
				var swapIndex = rng.Next(i + 1);
				var tmp = elements[i];
				elements[i] = elements[swapIndex];
				elements[swapIndex] = tmp;
			}
			// Lazily yield (avoiding aliasing issues etc). What?
			foreach (T element in elements) {
				yield return element;
			}
		}
	}
}
