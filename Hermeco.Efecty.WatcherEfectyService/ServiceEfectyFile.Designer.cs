using System.IO;
namespace Hermeco.Efecty.WatcherEfectyService
{
    partial class ServiceEfectyFile
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
            this.fileSystemWatcherEfecty = new System.IO.FileSystemWatcher();
            ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcherEfecty)).BeginInit();
            // 
            // fileSystemWatcherEfecty
            // 
            this.fileSystemWatcherEfecty.EnableRaisingEvents = true;
            this.fileSystemWatcherEfecty.Changed += new System.IO.FileSystemEventHandler(this.fileSystemWatcherEfecty_Changed);
            // 
            // ServiceEfectyFile
            // 
            this.ServiceName = "ServiceEfecty";
            ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcherEfecty)).EndInit();

        }

        private void fileSystemWatcherEfecty_Changed(object sender, FileSystemEventArgs e)
        {
           // throw new System.NotImplementedException();
        }

        #endregion

      
        private System.IO.FileSystemWatcher fileSystemWatcherEfecty;
    }
}
