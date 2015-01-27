﻿#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.ObjectModel;
using System.Data.Entity.Core;
using System.IO;
using System.Linq;
using AppsTracker.Controls;
using AppsTracker.DAL;
using AppsTracker.Utils;
using Microsoft.VisualBasic.ApplicationServices;

namespace AppsTracker
{
    public static class EntryPoint
    {
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                ConnectionConfig.CheckConnection();
            }
            catch (System.IO.IOException ex)
            {
                Exceptions.FileLogger.Log(ex);
                System.Windows.Forms.MessageBox.Show("Database folder creation failed, check error log for more information.", Constants.APP_NAME);
                return;
            }
            catch (System.Security.SecurityException ex)
            {
                Exceptions.FileLogger.Log(ex);
                System.Windows.Forms.MessageBox.Show("Database creation forbidden./nConnection string is not encrypted.", Constants.APP_NAME);
                return;
            }

            System.Data.Entity.Database.SetInitializer<AppsTracker.DAL.AppsEntities>(new AppsTracker.DAL.AppsDataBaseInitializer());

            ConnectionConfig.ToggleConfigEncryption();

            try
            {
                using (AppsEntities context = new AppsEntities())
                {
                    var s = context.Settings.FirstOrDefault();
                    context.SaveChanges();
                }
            }
            catch (EntityException ee)
            {
                Exceptions.FileLogger.Log(ee);
                if (ee.Message == "The underlying provider failed on Open.")
                {
                    string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AppService", "apps.sdf");
                    string message = "";
                    if (File.Exists(path))
                        message = string.Format("Error occured while trying to access the database!\nDeleting the database in path {0} will force the app to create a new database.\n\n WARNING! \n All data in the existing database will be lost!", path);
                    else
                        message = "Error occured while trying to access the database!\n" + ee.Message + "\nCheck ErrorLog.log for more details";
                    System.Windows.Forms.MessageBox.Show(message, Constants.APP_NAME);
                }
                return;
            }
            catch (Exception ex)
            {
                Exceptions.FileLogger.Log(ex);
                System.Windows.Forms.MessageBox.Show("Error occured while trying to access the database!\n" + ex.Message + "\nCheck ErrorLog.log for more details", Constants.APP_NAME);
                return;
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#if DEBUG
            RunApp(new ReadOnlyCollection<string>(args));
#else

            SingleInstanceManager singleInstanceApp = new SingleInstanceManager();
            singleInstanceApp.Run(args);
#endif
        }

       private static void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            var app = App.Current as App;
            try
            {
                Exception ex = e.ExceptionObject as Exception;
                AppsTracker.Exceptions.FileLogger.Log(ex);
                if (App.UzerSetting != null)
                {
                    if (!App.UzerSetting.Stealth)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            MessageWindow messageWindow = new MessageWindow("Ooops, this is awkward ... something went wrong." +
                                Environment.NewLine + "The app needs to close." + Environment.NewLine + "Error: " + ex.Message);
                            messageWindow.ShowDialog();
                        }));
                    }
                }
            }
            finally
            {
                if (app != null)
                    app.FinishAndExit();
            }
        }

       private static void RunApp(ReadOnlyCollection<String> eventArgs)
       {
           if (eventArgs.Count == 0)
           {
               System.Windows.SplashScreen splashScreen = new System.Windows.SplashScreen("resources/appstrackersplashresized.png");
               splashScreen.Show(true);
           }
           App app = new App(eventArgs);
           app.Run();
       }

        public class SingleInstanceManager : WindowsFormsApplicationBase
        {
            public static event EventHandler SecondInstanceActivating;

            public SingleInstanceManager()
            {
                this.IsSingleInstance = true;
            }

            protected override bool OnStartup(StartupEventArgs eventArgs)
            {
                RunApp(eventArgs.CommandLine);
                return false;
            }

            protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
            {
                base.OnStartupNextInstance(eventArgs);
                SecondInstanceActivating.InvokeSafely(this, EventArgs.Empty);
            }
        }
    }
}
