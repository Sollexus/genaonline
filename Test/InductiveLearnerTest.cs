using System;
using System.Diagnostics;
using System.Linq;
using Combinatorics.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SollexMachineLearning;
using System.Collections.Generic;


namespace Test {
	[TestClass]
	public class InductiveLearnerTest {
		[TestMethod]
		public void ExcelToInputData() {
			//var fileName = @"..\..\..\..\Кодирование процедура_ОВ_нов._Град_х5х6_с другой ЭВ для града.xls";
			//InductiveLearner.ProcessExcelFile(fileName, 7, 102, sheetName: "Экзамен");
		}

		[TestMethod]
		public void TestCombinatorics() {
			var list = new [] { 0, 1 };

			var c = new Variations<int>(list, 3, GenerateOption.WithRepetition);

			c.ToList().ForEach(row => Debug.WriteLine(String.Join(",", row)));
		}

		public void TestReturnEmumInEnumerable() {
			var list = new[] {"1", "2", "Wtf"};
		}
	}
}
