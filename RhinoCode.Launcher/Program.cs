// File: RhinoCode.Launcher\Program.cs
using System;

namespace RhinoCode.Launcher
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Launcher exists to debug RhinoCode.Tests via Rhino.exe
            Console.WriteLine("Launcher started - RhinoCode.Tests should run via debug profile.");
        }
    }
}