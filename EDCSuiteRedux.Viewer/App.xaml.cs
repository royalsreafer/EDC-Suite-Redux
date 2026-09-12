using System.Windows;
using System.Windows.Threading;

namespace EDCSuiteRedux.Viewer;

public partial class App : Application
{
	protected override void OnStartup(StartupEventArgs e)
	{
		DispatcherUnhandledException += OnDispatcherUnhandledException;

		try
		{
			MainWindow = new MainWindow();
			MainWindow.Show();
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.ToString(), "Viewer startup failed", MessageBoxButton.OK, MessageBoxImage.Error);
			Shutdown(1);
			return;
		}

		base.OnStartup(e);
	}

	private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
	{
		MessageBox.Show(e.Exception.ToString(), "Viewer error", MessageBoxButton.OK, MessageBoxImage.Error);
		e.Handled = true;
	}
}