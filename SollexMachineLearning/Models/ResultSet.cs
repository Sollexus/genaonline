using System.Collections.Generic;

namespace SollexMachineLearning.Models {
	public class ResultSet {
		public List<TestMatrixItem> TestMatrix { get; set; }
		public List<DiscretizedDataRow> DiscretizedInput { get; set; }
		public DecisionColumn DecisionColumn { get; set; }
	}
}