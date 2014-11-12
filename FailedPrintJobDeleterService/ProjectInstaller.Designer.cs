namespace FailedPrintJobDeleterService
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TheServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.TheServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // TheServiceProcessInstaller
            // 
            this.TheServiceProcessInstaller.Password = null;
            this.TheServiceProcessInstaller.Username = null;
            // 
            // TheServiceInstaller
            // 
            this.TheServiceInstaller.Description = "Periodically deletes failed print jobs from printers.";
            this.TheServiceInstaller.DisplayName = "FailedPrintJobDeleter";
            this.TheServiceInstaller.ServiceName = "FailedPrintJobDeleter";
            this.TheServiceInstaller.ServicesDependedOn = new string[] {
        "Tcpip"};
            this.TheServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.TheServiceProcessInstaller,
            this.TheServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller TheServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller TheServiceInstaller;
    }
}
