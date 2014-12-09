using System.Windows;
using Sollex.Properties;

namespace Sollex {
	public partial class App {
		protected override void OnExit(ExitEventArgs e) {
			base.OnExit(e);
			Settings.Default.Save();
		}
	}
}