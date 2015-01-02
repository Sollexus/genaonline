namespace SollexMachineLearning.Models {
	public class LearningParams {
		public int TriesCount = 10;
		public float LearningSetPercentage = 0.5f;
		public int IntervalsCount = 5; // the number of gradations for dizretization of float inputs
		public bool IsDiscretized;
		public string ColumnsToTake;
	}
}