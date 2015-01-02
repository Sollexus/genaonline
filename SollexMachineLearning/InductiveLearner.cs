using System;
using System.Collections.Generic;
using Combinatorics.Collections;
using System.Linq;
using SollexMachineLearning.Models;

namespace SollexMachineLearning {
	public static class InductiveLearner {
		public static ResultSet LoadCsvFile(string fileContent, CsvLoadingParams prms) {
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

			return ProcessInput(res);
		}

		private static ResultSet ProcessInput(List<MLInputDataRow> dataList) {
			//doing the work
			var prms = new LearningParams();

			var rowsCount = dataList.Count;
			var inputsCount = dataList[0].Inputs.Count();

			var trainingSetSize = (int) (rowsCount*prms.LearningSetPercentage);
			var testSetSize = rowsCount - trainingSetSize;
			var dimensionsList = Enumerable.Range(0, inputsCount).ToList();
			for (var i = 0; i < prms.TriesCount; i++) {
				var shuffledList = dataList.Shuffle().ToList();
				var trainingSet = shuffledList.Take(trainingSetSize).ToList();
				var testSet = shuffledList.Take(testSetSize).ToList();

				var votes = new int[testSetSize, 2]; //to group info for a decisive function

				List<DiscretizedDataRow> trainingDiscretizedSet;
				List<DiscretizedDataRow> testDiscretizedSet;

				#region "Discretization"

				var minMaxes = Enumerable.Range(0, inputsCount)
					.Select(j => {
						var min = trainingSet.Select(row => row.Inputs.Skip(j).First()).Min();
						var max = trainingSet.Select(row => row.Inputs.Skip(j).First()).Max();
						var interval = (max - min)/(prms.IntervalsCount - 1);
						return new {min, interval, max};
					}).ToArray();

				Func<MLInputDataRow, DiscretizedDataRow> discretizationFunc = row => new DiscretizedDataRow {
					Inputs = row.Inputs.Select((inp, j) => {
						if (inp <= minMaxes[j].min) return 0;
						if (inp >= minMaxes[j].max) return prms.IntervalsCount - 1;
						return (int) Math.Floor((inp - minMaxes[j].min)/minMaxes[j].interval);
					}).ToArray(),
					Class = row.Class
				};

				Func<MLInputDataRow, DiscretizedDataRow> noDiscretizationFunc = row => new DiscretizedDataRow {
					Inputs = row.Inputs.Select(inp => (int) inp).ToArray(),
					Class = row.Class
				};

				if (!prms.IsDiscretized) {
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
					var variations = new Variations<int>(Enumerable.Range(0, prms.IntervalsCount - 1).ToList(), d,
						GenerateOption.WithRepetition);

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

				#region "Write test matrix into result"

				var decision = new DecisionColumn(testSetSize);

				for (var j = 0; j < testSetSize; j++) {
					var res = votes[j, 0] >= votes[j, 1] ? 0 : 1;
					decision.Results[j] = res;
					if (res == testDiscretizedSet[j].Class) decision.OverallResult++;
				}

				return new ResultSet {DiscretizedInput = testDiscretizedSet, TestMatrix = testMatrix, DecisionColumn = decision};

				#endregion
			}

			return null;
		}
	}
}