using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using GenaOnline.Models;
using SollexMachineLearning;

namespace GenaOnline.Controllers {
	public class HomeController : Controller {
		public ActionResult Index() {
			return RedirectToAction("LoadFile");
		}

		public ActionResult LoadFile() {
			return View();
		}

		[HttpPost]
		public ActionResult LoadFile(LearningParamsViewModel prms) {
			if (!ModelState.IsValid) return View(prms);

			using (var reader = new StreamReader(prms.CsvFile.InputStream)) {
				int[] columnsToTake = null;

				try {
					columnsToTake = prms.ColumnsToTake.Split(',').Select(int.Parse).ToArray();
				} catch (Exception ex) {
					ModelState.AddModelError("ColumnsToTake", ex);
				}

				var loadingParams = new CsvLoadingParams {
					ColumnsToTake = columnsToTake,
					InputsCount = prms.InputsCount
				};

				var res = InductiveLearner.LoadCsvFile(reader.ReadToEnd(), loadingParams);
				return View("LoadedFile", res);
			}
		}
	}
}