using System;
using System.Diagnostics;
using System.Management.Automation.Runspaces;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using PowerShellEngine;

namespace PowerWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// The current application
        /// </summary>
        private App myApp;

        /// <summary>
        /// Error Message holder
        /// </summary>
        private string errorMessage;

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            myApp = Application.Current as App;
            LoadEvents();
            MyCustomTool.Initialize();
        }

        /// <summary>
        /// Loads PowerShell Engine related events (pipeline/runspace states)
        /// </summary>
        private void LoadEvents()
        {
            myApp.PoshEngine.PipelineErrorRaised += new PowerShellHelper.PipelineErrorEventHandler(poshEngine_PipelineErrorRaised);
            myApp.PoshEngine.PipelineStateChangedReceived += new PowerShellHelper.PipelineStateChangedEventHandler(poshEngine_PipelineStateChangedReceived);
            myApp.PoshEngine.PowerShellErrorRaised += new PowerShellHelper.PowerShellErrorEventHandler(poshEngine_PowerShellErrorRaised);
            myApp.PoshEngine.RunspaceStateChangedReceived += new PowerShellHelper.RunspaceStateChangedEventHandler(poshEngine_RunspaceStateChangedReceived);
            myApp.PoshEngine.ScriptStarted += new EventHandler(PoshEngine_ExecutionStarted);
        }

        /// <summary>
        /// Triggered when the script execution start
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PoshEngine_ExecutionStarted(object sender, EventArgs e)
        {
            PipelineStatusTextBlock.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(ScriptStarted));
        }

        /// <summary>
        /// Update UI for the ExecutionStarted event
        /// </summary>
        private void ScriptStarted()
        {
            errorMessage = string.Empty;
            RunspaceExclamationImage.Visibility = Visibility.Hidden;
            PipelineExclamationImage.Visibility = Visibility.Hidden;
            RunspaceBorder.Background = new SolidColorBrush(Colors.Transparent);
            PipelineBorder.Background = new SolidColorBrush(Colors.Transparent);
            StatusProgress.Minimum = -10;
            StatusProgress.IsIndeterminate = true;
            //MainStatusBar.Background = new SolidColorBrush(Colors.Orange);
        }

        /// <summary>
        /// Triggered when the runspace state changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arg"></param>
        void poshEngine_RunspaceStateChangedReceived(object sender, RunspaceStateEventArg arg)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => RunspaceStateChanged(arg)));
        }

        private void RunspaceStateChanged(RunspaceStateEventArg arg)
        {
            RunspaceStatusTextBlock.Text = arg.State.ToString();

            if (arg.State == RunspaceState.Opening || arg.State == RunspaceState.Closing)
            {
                RunspaceBorder.Background = new SolidColorBrush(Colors.Orange);
            }
            else if (arg.State == RunspaceState.Opened)
            {
                RunspaceBorder.Background = new SolidColorBrush(Colors.LightGreen);
            }
            else if (arg.State == RunspaceState.Closed)
            {   
                StatusProgress.IsIndeterminate = false;
                RunspaceBorder.Background = new SolidColorBrush(Colors.Transparent);
            }
            else if (arg.State == RunspaceState.Broken)
            {
                StatusProgress.IsIndeterminate = false;
                RunspaceBorder.Background = new SolidColorBrush(Colors.Red);
            }
        }

        /// <summary>
        /// Trigerred when an error was catched on the PowerShell object level
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arg"></param>
        void poshEngine_PowerShellErrorRaised(object sender, PowerShellErrorEventArg arg)
        {
            this.Dispatcher.Invoke(new Action(() => PowerShellError(arg)));
        }

        /// <summary>
        /// Update UI for the PowerShellErrorRaised event
        /// </summary>
        /// <param name="arg"></param>
        private void PowerShellError(PowerShellErrorEventArg arg)
        {
            RunspaceExclamationImage.Visibility = System.Windows.Visibility.Visible;
            RunspaceStatusTextBlock.Text = "Error";
            StatusProgress.IsIndeterminate = false;
            errorMessage = "PowerShell Error:\r\n" + arg.PowerShellException.Message + "\r\nInner Exception: " +
                           arg.PowerShellException.InnerException;
        }
     
        /// <summary>
        /// Triggered when the PowerShell pipeline state changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arg"></param>
        void poshEngine_PipelineStateChangedReceived(object sender, PipelineStateEventArg arg)
        {
            PipelineStatusTextBlock.Dispatcher.Invoke(new Action(() => PipelineStatusTextBlock.Text = arg.State.ToString()));

            if (arg.State == PipelineState.Stopped || arg.State == PipelineState.Failed || arg.State == PipelineState.Completed)
            {
                this.Dispatcher.Invoke(new Action(ScriptFinished));
            }
            else if (arg.State == PipelineState.Stopping)
            {
                this.Dispatcher.Invoke(new Action(() => PipelineBorder.Background = new SolidColorBrush(Colors.Orange)));
            }
            else if (arg.State == PipelineState.Running)
            {
                this.Dispatcher.Invoke(new Action(() => PipelineBorder.Background = new SolidColorBrush(Colors.LightGreen)));
            }
        }

        /// <summary>
        /// Update UI when the script is finished (pipeline stopped/failed/completed, runspace closed)
        /// </summary>
        private void ScriptFinished()
        {
            StatusProgress.IsIndeterminate = false;
            MainStatusBar.Background = new SolidColorBrush(Colors.LightGray);
            RunspaceBorder.Background = new SolidColorBrush(Colors.Transparent);
            PipelineBorder.Background = new SolidColorBrush(Colors.Transparent);
            StatusProgress.Minimum = 0;
        }

        /// <summary>
        /// Triggered when an error happened at the pipeline object level
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arg"></param>
        void poshEngine_PipelineErrorRaised(object sender, PipelineErrorEventArg arg)
        {
            PipelineStatusTextBlock.Dispatcher.Invoke(new Action(() => RaisePipelineError(arg.Error)));
        }

        /// <summary>
        /// Update UI for the PipelineError event
        /// </summary>
        /// <param name="message"></param>
        private void RaisePipelineError(string message)
        {
            PipelineExclamationImage.Visibility = System.Windows.Visibility.Visible;
            PipelineBorder.Background = new SolidColorBrush(Colors.Red);
            PipelineStatusTextBlock.Text = "Error";
            errorMessage = message;
        }

        /// <summary>
        /// Displays the error text when the user click on the exclamation mark
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExclamationImage_Click(object sender, RoutedEventArgs e)
        {
            if (errorMessage != string.Empty)
            {
                MessageBox.Show(this, errorMessage, "Execution Error", MessageBoxButton.OK);
            }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
