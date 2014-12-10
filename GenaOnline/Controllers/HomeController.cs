using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SollexMachineLearning;

namespace GenaOnline.Controllers {
	public class HomeController : Controller {
		public ActionResult Index() {
			return View();
		}

		[HttpPost]
		public ActionResult LoadFile(HttpPostedFileBase file, int inputsCount, string columnsToTake) {
			using (var reader = new StreamReader(file.InputStream)) {
				var prms = new CsvLoadingParams {
					ColumnsToTake = columnsToTake.Split(',').Select(int.Parse).ToArray(),
					InputsCount = inputsCount
				};

				var res = InductiveLearner.LoadCsvFile(reader.ReadToEnd(), prms);
				return View(res);
			}
		}
	}
}