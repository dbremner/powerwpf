using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PowerShellEngine;

namespace PowerWPF
{
    /// <summary>
    /// Interaction logic for MyTool.xaml
    /// </summary>
    public partial class MyTool : UserControl
    {   
        /// <summary>
        /// Current application object
        /// </summary>
        private App myApp;

        /// <summary>
        /// PowerShell Engine reference
        /// </summary>
        private PowerShellHelper poshEngine;

        /// <summary>
        /// Constructor
        /// </summary>
        public MyTool()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes objects and output from pipeline event
        /// </summary>
        public void Initialize()
        {
            myApp = Application.Current as App;
            poshEngine = myApp.PoshEngine;
            poshEngine.PipelineOutputReceived += new PowerShellHelper.PipelineOutputEventHandler(poshEngine_PipelineOutputReceived);
        }

        // Put your own code below
        #region CustomCode

        /// <summary>
        /// Triggered when Execute button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DirectCodeButton_Click(object sender, RoutedEventArgs e)
        {
            // Clean outputbox
            OutputBox.Text = string.Empty;
            // Get script code from InputBox
            string script = InputBox.Text;
            // Call ExecuteScript method
            poshEngine.ExecuteScript(script);
        }

        /// <summary>
        /// Triggered when data arrived from the pipeline
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arg"></param>
        void poshEngine_PipelineOutputReceived(object sender, PipelineOutputEventArg arg)
        {
            // Get the base object from pipeline output, convert it to string
            string result = arg.PSObjectOutput.BaseObject.ToString();

            // Invoke SetResult Method
            this.Dispatcher.Invoke(new Action(() => SetResult(result)));
        }

        /// <summary>
        /// Invoked from the PipelineOutputReceived event
        /// </summary>
        /// <param name="result"></param>
        private void SetResult(string result)
        {
            // Add Text from the pipeline output to the outputbox
            OutputBox.Text = OutputBox.Text + "\r\n" + result;
        }

        /// <summary>
        /// Triggered when TextFileButton is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextFileButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear the outputbox
            OutputBox.Text = string.Empty;

            // Specify the script path in the build
            string ScriptPath = "Scripts/MyScript.txt";

            // Get the script code
            string script = Utilities.GetScriptFileCode(ScriptPath);

            // Replace %Value% with selected item in the ListBox
            var selectedItem = (ListBoxItem) ParametersListbox.SelectedItem;
            script = script.Replace(@"%Value%", selectedItem.Content.ToString());

            // Execute Script
            poshEngine.ExecuteScript(script);
        }

        #endregion
    }
}
