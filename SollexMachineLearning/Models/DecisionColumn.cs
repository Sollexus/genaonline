namespace SollexMachineLearning.Models {
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
