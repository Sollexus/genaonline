using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using SollexMachineLearning;

namespace Sollex {
	public partial class MainPage : Page {
		public MainPage() {
			InitializeComponent();

			LoadSettings();
		}

		private void LoadSettings() {
			TxtInputsCount.Text = Properties.Settings.Default.InputsCount.ToString();
			TxtRowsCount.Text = Properties.Settings.Default.RowsCount.ToString();
			TxtRowsOffset.Text = Properties.Settings.Default.RowsOffset.ToString();
			TxtColsOffset.Text = Properties.Settings.Default.ColsOffset.ToString();
			TxtSheetName.Text = Properties.Settings.Default.SheetName;
			TxtLearningSetSize.Text = Properties.Settings.Default.LearningSetSize.ToString();
			TxtTriesCount.Text = Properties.Settings.Default.TriesCount.ToString();
            TxtIntervalsCount.Text = Properties.Settings.Default.IntervalsCount.ToString();
			InitIsInitializedCombobox();
			TxtColumnsToTake.Text = Properties.Settings.Default.ColumnsToTake;
		}

		private void InitIsInitializedCombobox() {
			CBIsDiscretized.SelectedValue = Properties.Settings.Default.IsDiscretized ? "True" : "False";
		}

		void BtnLoadFile_Click(object sender, RoutedEventArgs e) {
			var dialog = new OpenFileDialog {DefaultExt = ".xls", Filter = "Excel files (*.xls,*.xlsx)|*.xls;*.xlsx"};

		    int inputsCount;
			if (!int.TryParse(TxtInputsCount.Text, out inputsCount)) {
				MessageBox.Show("Inputs count should be a decimal value");
				return;
			}

			int rowsCount;
			if (!int.TryParse(TxtRowsCount.Text, out rowsCount)) {
				MessageBox.Show("Rows count should be a decimal value");
				return;
			}

			int rowsOffset;
			if (!int.TryParse(TxtRowsOffset.Text, out rowsOffset)) {
				MessageBox.Show("Rows offset should be a decimal value");
				return;
			}

			int colsOffset;
			if (!int.TryParse(TxtColsOffset.Text, out colsOffset)) {
				MessageBox.Show("Columns offset should be a decimal value");
				return;
			}

			float learningSetSize;
			if (!float.TryParse(TxtLearningSetSize.Text, out learningSetSize)) {
				MessageBox.Show("Learning set size should be a floating point number specifying the percentage of the learning set");
				return;
			}
			if (learningSetSize > 100) {
				MessageBox.Show("Learning set size can't be bigger than 100");
				return;
			}

			int triesCount;
			if (!int.TryParse(TxtTriesCount.Text, out triesCount)) {
				MessageBox.Show("Tries count should be a decimal value");
				return;
			}

		    int intervalsCount;
		    if (!int.TryParse(TxtIntervalsCount.Text, out intervalsCount)) {
		        MessageBox.Show("Intervals count should be a decimal value");
		        return;
		    }
		    if (intervalsCount < 2) {
                MessageBox.Show("Intervals count should be a decimal value bigger than 1");
                return;
		    }

			var isDiscretized = (string) CBIsDiscretized.SelectedValue == "True";
			var columnsToTake = TxtColumnsToTake.Text;

			if (dialog.ShowDialog() == true) {
				
				InductiveLearner.TriesCount = triesCount;
				InductiveLearner.LearningSetPercentage = learningSetSize/100;
			    InductiveLearner.IntervalsCount = intervalsCount;
				InductiveLearner.IsDiscretized = isDiscretized;
				InductiveLearner.ColumnsToTake = columnsToTake;
				
                try {
                    InductiveLearner.ProcessExcelFile(dialog.FileName, inputsCount, rowsCount, TxtSheetName.Text, colsOffset, rowsOffset);
                } catch (Exception ex) {
                    MessageBox.Show(ex.Message + (ex.InnerException != null ? ex.InnerException.Message : "") + "\n" + ex.StackTrace);
                    return;
                }
				MessageBox.Show("Operation completed");
			}

			Properties.Settings.Default.InputsCount = inputsCount;
			Properties.Settings.Default.RowsCount = rowsCount;
			Properties.Settings.Default.RowsOffset = rowsOffset;
			Properties.Settings.Default.ColsOffset = colsOffset;
			Properties.Settings.Default.SheetName = TxtSheetName.Text;
			Properties.Settings.Default.LearningSetSize = learningSetSize;
			Properties.Settings.Default.TriesCount = triesCount;
		    Properties.Settings.Default.IntervalsCount = intervalsCount;
		    Properties.Settings.Default.IsDiscretized = isDiscretized;
			Properties.Settings.Default.ColumnsToTake = columnsToTake;
		}
	}
}