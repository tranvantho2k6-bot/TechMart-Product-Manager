using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TechMartProductManager
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}
