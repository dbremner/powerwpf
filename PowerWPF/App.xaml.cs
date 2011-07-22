﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using PowerShellEngine;

namespace PowerWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public PowerShellHelper PoshEngine = new PowerShellHelper();
    }
}
