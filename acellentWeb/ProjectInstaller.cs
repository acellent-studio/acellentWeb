using System;
using System.Collections;
using System.ComponentModel;
using System.ServiceProcess;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

namespace acellentWeb
{    /// <summary>
     /// AcellentWebService 的安裝類別，繼承自 System.Configuration.Install.Installer。
     /// </summary>
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        /// <summary>
        /// AcellentWebService 的 Project Installer 建構子
        /// </summary>
        public ProjectInstaller()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 覆寫 ProjectInstaller 的 OnBeforeInstall 方法，自定義執行程式安裝前需執行的動作。
        /// </summary>
        /// <param name="savedState">傳入 System.Collections.IDictionary 型態的界面 (interface)</param>
        protected override void OnBeforeInstall(IDictionary savedState)
        {
            base.OnBeforeInstall(savedState);

            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController service in services)
            {
                //if (service.ServiceName == "AcellentWebService" && service.Status == ServiceControllerStatus.Running)
                if (service.ServiceName == "AcellentWebService")
                {
                    //MessageBox.Show(service.ServiceName + " found, trying to stop it.");
                    //service.MachineName = "tom-nb";
                    if (service.CanStop)
                    {
                        try
                        {
                            service.Stop();
                            service.WaitForStatus(ServiceControllerStatus.Stopped);
                            //service.Refresh();
                            MessageBox.Show("開始解除安裝舊版本 " + service.ServiceName);
                            //Process.Start("msiexec.exe", @"/uninstall {CF26A293-B3EC-402A-A40C-EDD5673152D0}").WaitForExit();
                            Process.Start("sc.exe", "delete \"AcellentWebService\"").WaitForExit();
                            MessageBox.Show("舊版本 " + service.ServiceName + " 已移除");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("AcellentWebService uninstall error:\r\n" + ex.ToString());
                        }
                    }
                    else
                    {
                        MessageBox.Show("無法停止 " + service.ServiceName + "，需自行至 Windows 服務管理員手動停用。");
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// 覆寫 ProjectInstaller 的 OnAfterInstall 方法，自定義執行程式安裝完成時需執行的動作。
        /// 此方法會嘗試啟動 acellentWebService 及 Web app 的主程式。
        /// </summary>
        /// <param name="savedState">傳入 System.Collections.IDictionary 型態的界面 (interface)</param>
        protected override void OnAfterInstall(IDictionary savedState)
        {
            base.OnAfterInstall(savedState);

            //The following code starts the service after it is installed.
            try
            {
                using (ServiceController serviceController = new System.ServiceProcess.ServiceController(serviceInstaller1.ServiceName))
                {
                    serviceController.Start();
                }

                // 如果使用者在安裝過程中選擇安裝完成後立即啟動 acellent 主程式，則...
                String doAction = this.Context.Parameters["runacellent"];
                // 當安裝選項中的 RUNANTOM 欄位為 checked 時，
                if (doAction.ToLower() == "checked")
                {
                    // 啟動 Acellent 主程式。
                    //Process.Start(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "acellent.exe"));
                    // 啟動 default browser 以執行 Acellent Web 主程式。
                    Process.Start(WebService.WebConfig.UrlSetting.GetFQDN);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("安裝過程發生錯誤 (OnAfterInstall):\n\n" + ex.ToString());
            }
        }


        /// <summary>
        /// 覆寫 ProjectInstaller 的 OnAfterRollback 方法，自定義當取消安裝程序需執行的安裝回復動作(Rollback)。
        /// 如果安裝沒成功而發生"取消安裝"，且之前系統中已有舊版 AcellentWebService 在在時，
        /// 則嘗試重新啟動在 OnBeforeInstall 中被 stop 的 windows service
        /// </summary>
        /// <param name="savedState">傳入 System.Collections.IDictionary 型態的界面 (interface)</param>
        protected override void OnAfterRollback(IDictionary savedState)
        {
            base.OnAfterRollback(savedState);
            try
            {
                using (System.ServiceProcess.ServiceController serviceController = new System.ServiceProcess.ServiceController(serviceInstaller1.ServiceName))
                {
                    serviceController.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Installation Error (OnAfterRollback):\n\n" + ex.ToString());
            }
        }
    }
}
