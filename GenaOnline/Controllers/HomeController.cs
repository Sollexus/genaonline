using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GenaOnline.Models;
using SollexMachineLearning;

namespace GenaOnline.Controllers {
	public class HomeController : Controller {
		public ActionResult Index() {
			return View(new LearningParamsViewModel());
		}

		[HttpPost]
		public ActionResult LoadFile(LearningParamsViewModel prms) {
			if (System.Web.Mvc.ModelState)

			using (var reader = new StreamReader(prms.CsvFile.InputStream)) {
				var loadingParams = new CsvLoadingParams {
					ColumnsToTake = prms.ColumnsToTake.Split(',').Select(int.Parse).ToArray(),
					InputsCount = prms.InputsCount
				};

				var res = InductiveLearner.LoadCsvFile(reader.ReadToEnd(), loadingParams);
				return View(res);
			}
		}
	}
}