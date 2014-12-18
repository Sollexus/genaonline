using System.Collections.Generic;

namespace SollexMachineLearning {
	public class DecisionColumn {
		public DecisionColumn() {
			
		}

		public DecisionColumn(int capacity) {
			Results = new int[capacity];
		}

		public int[] Results { get; set; }
		public int OverallResult { get; set; }
	}
}
