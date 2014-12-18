using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GenaOnline.Models {
	public class LearningParamsViewModel {
		[Required]
		public int InputsCount { get; set; }
		
		public string ColumnsToTake { get; set; }

		[Required]
		[DataType(DataType.Upload)]
		public HttpPostedFileBase CsvFile { get; set; }
	}
}