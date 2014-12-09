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
using SollexMachineLearning;

namespace Sollex {
    public partial class DiscretizedPage : Page {
        public DiscretizedPage(IEnumerable<DiscretizedDataRow> data) {
            InitializeComponent();

            FillGrid(data);
        }

        private void FillGrid(IEnumerable<DiscretizedDataRow> data) {
            var list = data.ToList();

            if (!list.Any()) {
                MessageBox.Show("Data is empty");
                return;
            }

            for (int i = 0; i < list[0].Inputs.Length; i++) {
                TblInput.Columns.Add(new DataGridTextColumn {
                    Header = "X" + i, Binding = new Binding(string.Format("Inputs[{0}]", i))
                });
            }

            TblInput.Columns.Add(new DataGridTextColumn {
                Header = "Class", Binding = new Binding("Class")
            });

            TblInput.ItemsSource = list;
        }
    }
}