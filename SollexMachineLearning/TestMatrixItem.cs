using System;
using System.Collections.Generic;
using System.Linq;

namespace SollexMachineLearning {
	public class TestMatrixItem {
		public int?[] TestColumn; //suggested by function values
		public List<int> Combination; // inputs indexes. For example [x6, x7, x9]
		public int Class; //function class
		public List<int> CombinationValues; //function input values
		public int[] ClassCounts; //numbers of items which fall in each class. For binary that would be two numbers like [25, 2]
		public int ValidCount; //number of items in test set which passed the test


		public string InputsToString() {
			return string.Format("[{0}]", String.Join(",", Combination.Select(c => string.Format("x{0}", c + 1))));
		}

		public string FunctionToString() {
			return string.Format("[{0}] = {1}", String.Join(",", CombinationValues.Select(c => c.ToString())), Class);
		}

		public string ClassCountsToString() {
			return string.Format("[{0}:{1}]", ClassCounts[0], ClassCounts[1]);
		}
	}
}