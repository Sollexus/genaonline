using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections;
using System.Windows.Navigation;
using SollexMachineLearning;

namespace Sollex {
    public partial class InputPage : Page {
        private List<MLInputDataRow> _dataList;

        public InputPage(IEnumerable<MLInputDataRow> data) {
            InitializeComponent();

            FillGrid(data);
        }

        private void FillGrid(IEnumerable<MLInputDataRow> data) {
            _dataList = data.ToList();

            if (!_dataList.Any()) {
                MessageBox.Show("Data is empty");
                return;
            }

            for (int i = 0; i < _dataList[0].Inputs.Length; i++) {
                TblInput.Columns.Add(new DataGridTextColumn {
                    Header = "X" + i, Binding = new Binding(string.Format("Inputs[{0}]", i))
                });
            }

            TblInput.Columns.Add(new DataGridTextColumn {
                Header = "Class", Binding = new Binding("Class")
            });

            TblInput.ItemsSource = _dataList;
        }

        private void BtnNext_OnClick(object sender, RoutedEventArgs e) {
            var list = _dataList.ToList();

            var avgs = Enumerable.Range(0, list[0].Inputs.Length)
                .Select(i => list.Select(row => row.Inputs.Skip(i).First()).Average()).ToArray();

            var discrDataRows = list.Select(row => new DiscretizedDataRow {
                Inputs = row.Inputs.Select((input, i) => (input >= avgs[i] ? 1 : 0)).ToArray(),
                Class = row.Class
            }).ToList();

            /*var significances = Enumerable.Range(0, list[0].Inputs.Length)
                .Select(i => discrDataRows.Select())*/
            NavigationService.Navigate(new DiscretizedPage(discrDataRows));
        }
    }
}