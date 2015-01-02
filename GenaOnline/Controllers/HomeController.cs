using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using GenaOnline.Models;
using SollexMachineLearning;
using SollexMachineLearning.Models;

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

				ResultSet res;

				try {
					res = InductiveLearner.LoadCsvFile(reader.ReadToEnd(), loadingParams);
				} catch (Exception ex) {
					var errReport = string.Format("{0}\n{1}", ex.Message, ex.StackTrace);
					return View("LoadingError", (object)errReport);
				}

				return View("LoadedFile", res);
			}
		}
	}
}