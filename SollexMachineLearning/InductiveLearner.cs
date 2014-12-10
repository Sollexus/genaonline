using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Combinatorics.Collections;
using System.Linq;
using OfficeOpenXml;

namespace SollexMachineLearning {
	public class MLInputDataRow {
		public float[] Inputs { get; set; }
		public int Class { get; set; }
	}

	public class DiscretizedDataRow {
		public int[] Inputs { get; set; }
		public int Class { get; set; }
	}

	internal class TestMatrixItem {
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

	public class LearningParams {
		public int TriesCount = 10;
		public float LearningSetPercentage = 0.5f;
		public int IntervalsCount = 5; // the number of gradations for dizretization of float inputs
		public bool IsDiscretized;
		public string ColumnsToTake;
	}

	public class CsvLoadingParams {
		public int InputsCount;
		public int[] ColumnsToTake;
	}
		
	public static class InductiveLearner {
		public static List<MLInputDataRow> LoadCsvFile(string fileContent, CsvLoadingParams prms) {
			var lines = fileContent.Split(Environment.NewLine.ToCharArray()).Skip(1);

			var columns = prms.ColumnsToTake ?? Enumerable.Range(0, prms.InputsCount).ToArray();

			var res = new List<MLInputDataRow>();

			foreach (var line in lines) {
				if (String.IsNullOrEmpty(line)) continue;

				var row = new MLInputDataRow {Inputs = new float[columns.Length]};

				var cells = line.Split(',');

				for (var i = 0; i < columns.Length; i++) {
					row.Inputs[i] = float.Parse(cells[columns[i]]);
				}

				row.Class = int.Parse(cells[prms.InputsCount]);

				res.Add(row);
			}

			return res;
		}

		/*public static void ProcessCsvFile(List<MLInputDataRow> data) {
			var dataList = LoadCsvFile(fileName, columnsToTake);


			/#1#/Output excel file
			var fileInfo = new FileInfo(@"Result.xlsx");
			if (fileInfo.Exists) fileInfo.Delete();

			var pck = new ExcelPackage(fileInfo);

			//doing the work
			var trainingSetSize = (int)(rowsCount * LearningSetPercentage);
			var testSetSize = rowsCount - trainingSetSize;
			var dimensionsList = Enumerable.Range(0, inputsCount).ToList();
			for (var i = 0; i < TriesCount; i++) {
				var shuffledList = dataList.Shuffle().ToList();
				var trainingSet = shuffledList.Take(trainingSetSize).ToList();
				var testSet = shuffledList.Take(testSetSize).ToList();
				var trainingWS = pck.Workbook.Worksheets.Add(string.Format("Training set {0}", i));
				var testWS = pck.Workbook.Worksheets.Add(string.Format("Test set {0}", i));
				var votes = new int[testSetSize, 2]; //to group info for a decisive function

				List<DiscretizedDataRow> trainingDiscretizedSet;
				List<DiscretizedDataRow> testDiscretizedSet;

				#region "Discretization"

				var minMaxes = Enumerable.Range(0, inputsCount)
					.Select(j => {
						var min = trainingSet.Select(row => row.Inputs.Skip(j).First()).Min();
						var max = trainingSet.Select(row => row.Inputs.Skip(j).First()).Max();
						var interval = (max - min) / (IntervalsCount - 1);
						return new { min, interval, max };
					}).ToArray();

				Func<MLInputDataRow, DiscretizedDataRow> discretizationFunc = row => new DiscretizedDataRow {
					Inputs = row.Inputs.Select((inp, j) => {
						if (inp <= minMaxes[j].min) return 0;
						if (inp >= minMaxes[j].max) return IntervalsCount - 1;
						return (int)Math.Floor((inp - minMaxes[j].min) / minMaxes[j].interval);
					}).ToArray(),
					Class = row.Class
				};

				Func<MLInputDataRow, DiscretizedDataRow> noDiscretizationFunc = row => new DiscretizedDataRow {
					Inputs = row.Inputs.Select(inp => (int)inp).ToArray(),
					Class = row.Class
				};

				if (!IsDiscretized) {
					trainingDiscretizedSet = trainingSet.Select(discretizationFunc).ToList();
					testDiscretizedSet = testSet.Select(discretizationFunc).ToList();
				} else {
					trainingDiscretizedSet = trainingSet.Select(noDiscretizationFunc).ToList();
					testDiscretizedSet = testSet.Select(noDiscretizationFunc).ToList();
				}

				#endregion

				//going through all the inputs combinations starting from 2
				var testMatrix = new List<TestMatrixItem>();
				for (var d = 2; d <= inputsCount; d++) {
					var combinations = new Combinations<int>(dimensionsList, d).ToList();
					var variations = new Variations<int>(Enumerable.Range(0, IntervalsCount - 1).ToList(), d, GenerateOption.WithRepetition);

					foreach (var comb in combinations) {
						foreach (var variation in variations) {
							var candidateItems = new List<TestMatrixItem>();
							var classCounts = new int[2];

							#region "Checking matrix items agains the training set"
							for (var cls = 0; cls <= 1; cls++) {
								var matrixItem = new TestMatrixItem {
									Combination = comb.ToList(),
									Class = cls,
									CombinationValues = variation.ToList(),
									TestColumn = new int?[testSetSize]
								};
								candidateItems.Add(matrixItem);

								for (var j = 0; j < trainingSetSize; j++) {
									var allInputsEqual = true;

									for (var k = 0; k < d; k++) {
										if (trainingDiscretizedSet[j].Inputs[comb[k]] == variation[k]) continue;
										allInputsEqual = false;
										break;
									}

									if (allInputsEqual && (trainingSet[j].Class == cls)) {
										classCounts[cls]++;
									}
								}


							}
							#endregion

							if ((classCounts[0] == 0) || (classCounts[1] == 0)) continue;

							var bestMatrixItem = candidateItems[classCounts[0] >= classCounts[1] ? 0 : 1];
							bestMatrixItem.ClassCounts = classCounts;
							testMatrix.Add(bestMatrixItem);

							//testing against the testing set
							for (var j = 0; j < testSetSize; j++) {
								var allInputsEqual = true;

								for (var k = 0; k < d; k++) {
									if (testDiscretizedSet[j].Inputs[comb[k]] == variation[k]) continue;
									allInputsEqual = false;
									break;
								}

								if (!allInputsEqual) continue;

								bestMatrixItem.TestColumn[j] = bestMatrixItem.Class;
								bestMatrixItem.ValidCount++;
								votes[j, bestMatrixItem.Class]++;
							}
						}
					}
				}

				//var validVotes = new int[testSetSize]; //number of rows where a decisive functiion was right at suggestion

				#region "Write test matrix into training worksheet"
				for (var j = 0; j < testMatrix.Count; j++) {
					trainingWS.Cells[trainingSetSize + 2, j + 1].Value = testMatrix[j].InputsToString();
					trainingWS.Cells[trainingSetSize + 3, j + 1].Value = testMatrix[j].FunctionToString();
					trainingWS.Cells[trainingSetSize + 4, j + 1].Value = testMatrix[j].ClassCountsToString();

					testWS.Cells[1, inputsCount + j + 2].Style.WrapText = true;
					testWS.Cells[1, inputsCount + j + 2].Value = testMatrix[j].InputsToString() + "\n" + testMatrix[j].FunctionToString() + "\n" +
												   testMatrix[j].ClassCountsToString();

					for (var k = 0; k < testSetSize; k++) {
						testWS.Cells[k + 2, inputsCount + j + 2].Value = testMatrix[j].TestColumn[k];
					}

					testWS.Cells[testSetSize + 2, inputsCount + j + 2].Value = testMatrix[j].ValidCount;
					testWS.Cells[testSetSize + 3, inputsCount + j + 2].Value = string.Format("{0:##.000}%", testMatrix[j].ValidCount * 100 / (float)testSetSize);
				}

				testWS.Cells[1, inputsCount + testMatrix.Count + 2].Value = "Decision";

				var validVotes = 0;
				for (var j = 0; j < testSetSize; j++) {
					var res = votes[j, 0] >= votes[j, 1] ? 0 : 1;
					testWS.Cells[j + 2, inputsCount + testMatrix.Count + 2].Value = res;
					if (res == testDiscretizedSet[j].Class) validVotes++;
				}

				testWS.Cells[testSetSize + 2, inputsCount + testMatrix.Count + 2].Value = validVotes;
				testWS.Cells[testSetSize + 3, inputsCount + testMatrix.Count + 2].Value =
					string.Format("{0:##.000}%", validVotes * 100 / (float)testSetSize); ;

				#endregion

				WriteToExcel(trainingWS, trainingDiscretizedSet);
				//WriteToExcel(trainingWS, significances, trainingSetSize + 2);
				WriteToExcel(testWS, testDiscretizedSet);
			}

			//dividing a data set into training and test sets

			pck.Save();#1#
		}*/

		public static void ProcessCsvFile(string fileContent, CsvLoadingParams prms) {
			var data = LoadCsvFile(fileContent, prms);
			var columns = prms.ColumnsToTake ?? Enumerable.Range(0, prms.InputsCount).ToArray();

			var dataList = data.ToList();
			var inputsCount = columns.Length;

			//Output excel file
			var fileInfo = new FileInfo(@"Result.xlsx");
			if (fileInfo.Exists) fileInfo.Delete();

			var pck = new ExcelPackage(fileInfo);

			//doing the work
			var trainingSetSize = (int)(rowsCount * LearningSetPercentage);
			var testSetSize = rowsCount - trainingSetSize;
			var dimensionsList = Enumerable.Range(0, inputsCount).ToList();
			for (var i = 0; i < TriesCount; i++) {
				var shuffledList = dataList.Shuffle().ToList();
				var trainingSet = shuffledList.Take(trainingSetSize).ToList();
				var testSet = shuffledList.Take(testSetSize).ToList();
				var trainingWS = pck.Workbook.Worksheets.Add(string.Format("Training set {0}", i));
				var testWS = pck.Workbook.Worksheets.Add(string.Format("Test set {0}", i));
				var votes = new int[testSetSize, 2]; //to group info for a decisive function

				List<DiscretizedDataRow> trainingDiscretizedSet;
				List<DiscretizedDataRow> testDiscretizedSet;

				#region "Discretization"

				var minMaxes = Enumerable.Range(0, inputsCount)
					.Select(j => {
						var min = trainingSet.Select(row => row.Inputs.Skip(j).First()).Min();
						var max = trainingSet.Select(row => row.Inputs.Skip(j).First()).Max();
						var interval = (max - min) / (IntervalsCount - 1);
						return new { min, interval, max };
					}).ToArray();

				Func<MLInputDataRow, DiscretizedDataRow> discretizationFunc = row => new DiscretizedDataRow {
					Inputs = row.Inputs.Select((inp, j) => {
						if (inp <= minMaxes[j].min) return 0;
						if (inp >= minMaxes[j].max) return IntervalsCount - 1;
						return (int)Math.Floor((inp - minMaxes[j].min) / minMaxes[j].interval);
					}).ToArray(),
					Class = row.Class
				};

				Func<MLInputDataRow, DiscretizedDataRow> noDiscretizationFunc = row => new DiscretizedDataRow {
					Inputs = row.Inputs.Select(inp => (int)inp).ToArray(),
					Class = row.Class
				};

				if (!IsDiscretized) {
					trainingDiscretizedSet = trainingSet.Select(discretizationFunc).ToList();
					testDiscretizedSet = testSet.Select(discretizationFunc).ToList();
				} else {
					trainingDiscretizedSet = trainingSet.Select(noDiscretizationFunc).ToList();
					testDiscretizedSet = testSet.Select(noDiscretizationFunc).ToList();
				}

				#endregion

				//going through all the inputs combinations starting from 2
				var testMatrix = new List<TestMatrixItem>();
				for (var d = 2; d <= inputsCount; d++) {
					var combinations = new Combinations<int>(dimensionsList, d).ToList();
					var variations = new Variations<int>(Enumerable.Range(0, IntervalsCount - 1).ToList(), d, GenerateOption.WithRepetition);

					foreach (var comb in combinations) {
						foreach (var variation in variations) {
							var candidateItems = new List<TestMatrixItem>();
							var classCounts = new int[2];

							#region "Checking matrix items agains the training set"
							for (var cls = 0; cls <= 1; cls++) {
								var matrixItem = new TestMatrixItem {
									Combination = comb.ToList(),
									Class = cls,
									CombinationValues = variation.ToList(),
									TestColumn = new int?[testSetSize]
								};
								candidateItems.Add(matrixItem);

								for (var j = 0; j < trainingSetSize; j++) {
									var allInputsEqual = true;

									for (var k = 0; k < d; k++) {
										if (trainingDiscretizedSet[j].Inputs[comb[k]] == variation[k]) continue;
										allInputsEqual = false;
										break;
									}

									if (allInputsEqual && (trainingSet[j].Class == cls)) {
										classCounts[cls]++;
									}
								}


							}
							#endregion

							if ((classCounts[0] == 0) || (classCounts[1] == 0)) continue;

							var bestMatrixItem = candidateItems[classCounts[0] >= classCounts[1] ? 0 : 1];
							bestMatrixItem.ClassCounts = classCounts;
							testMatrix.Add(bestMatrixItem);

							//testing against the testing set
							for (var j = 0; j < testSetSize; j++) {
								var allInputsEqual = true;

								for (var k = 0; k < d; k++) {
									if (testDiscretizedSet[j].Inputs[comb[k]] == variation[k]) continue;
									allInputsEqual = false;
									break;
								}

								if (!allInputsEqual) continue;

								bestMatrixItem.TestColumn[j] = bestMatrixItem.Class;
								bestMatrixItem.ValidCount++;
								votes[j, bestMatrixItem.Class]++;
							}
						}
					}
				}

				//var validVotes = new int[testSetSize]; //number of rows where a decisive functiion was right at suggestion

				#region "Write test matrix into training worksheet"
				for (var j = 0; j < testMatrix.Count; j++) {
					trainingWS.Cells[trainingSetSize + 2, j + 1].Value = testMatrix[j].InputsToString();
					trainingWS.Cells[trainingSetSize + 3, j + 1].Value = testMatrix[j].FunctionToString();
					trainingWS.Cells[trainingSetSize + 4, j + 1].Value = testMatrix[j].ClassCountsToString();

					testWS.Cells[1, inputsCount + j + 2].Style.WrapText = true;
					testWS.Cells[1, inputsCount + j + 2].Value = testMatrix[j].InputsToString() + "\n" + testMatrix[j].FunctionToString() + "\n" +
												   testMatrix[j].ClassCountsToString();

					for (var k = 0; k < testSetSize; k++) {
						testWS.Cells[k + 2, inputsCount + j + 2].Value = testMatrix[j].TestColumn[k];
					}

					testWS.Cells[testSetSize + 2, inputsCount + j + 2].Value = testMatrix[j].ValidCount;
					testWS.Cells[testSetSize + 3, inputsCount + j + 2].Value = string.Format("{0:##.000}%", testMatrix[j].ValidCount * 100 / (float)testSetSize);
				}

				testWS.Cells[1, inputsCount + testMatrix.Count + 2].Value = "Decision";

				var validVotes = 0;
				for (var j = 0; j < testSetSize; j++) {
					var res = votes[j, 0] >= votes[j, 1] ? 0 : 1;
					testWS.Cells[j + 2, inputsCount + testMatrix.Count + 2].Value = res;
					if (res == testDiscretizedSet[j].Class) validVotes++;
				}

				testWS.Cells[testSetSize + 2, inputsCount + testMatrix.Count + 2].Value = validVotes;
				testWS.Cells[testSetSize + 3, inputsCount + testMatrix.Count + 2].Value =
					string.Format("{0:##.000}%", validVotes * 100 / (float)testSetSize); ;

				#endregion

				WriteToExcel(trainingWS, trainingDiscretizedSet);
				//WriteToExcel(trainingWS, significances, trainingSetSize + 2);
				WriteToExcel(testWS, testDiscretizedSet);
			}

			//dividing a data set into training and test sets

			pck.Save();
			return;
		}
	}
}


//*********************************************** Outdated code****************************************************************
/*
				#region "Calculating significances"
				//TODO: Should check if that a correct way to calculate significance. Or does it really matter?
				var significances = Enumerable.Range(0, inputsCount)
					.Select(j => (Math.Max(
									trainingDiscretizedSet.Count(row => (row.Inputs[j] == 0 && row.Class == 1)),
									trainingDiscretizedSet.Count(row => (row.Inputs[j] == 0 && row.Class == 0)))
								  + Math.Max(
									trainingDiscretizedSet.Count(row => (row.Inputs[j] == 1 && row.Class == 1)),
									trainingDiscretizedSet.Count(row => (row.Inputs[j] == 1 && row.Class == 0)))) / (float)trainingSetSize
					).ToArray();
				#endregion

*/