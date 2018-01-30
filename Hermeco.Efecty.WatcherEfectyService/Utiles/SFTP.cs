using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tamir.SharpSsh;
using Tamir.SharpSsh.java.lang;

namespace Hermeco.Efecty.WatcherEfectyService.Utiles
{
    public class SFTP
    {
        static void sftp_OnTransferStart(string src, string dst, int transferredBytes, int totalBytes, string message)
        {
            //do nothing
        }

        static void sftp_OnTransferEnd(string src, string dst, int transferredBytes, int totalBytes, string message)
        {
            //do nothing
        }


        public static int UploadFile(string server, string user, string pass, string origen, string rutadestino, string nombredestino)
        {
            int returnVal = 0;

            for (int i = 0; i < 5; i++)
            {
                Thread.sleep(2000);
                //sftpClient = new tss.Sftp(server, user);
                Sftp sftp = new Sftp(server, user, pass);
                sftp.OnTransferStart += new Tamir.SharpSsh.FileTransferEvent(sftp_OnTransferStart);
                sftp.OnTransferEnd += new Tamir.SharpSsh.FileTransferEvent(sftp_OnTransferEnd);
                try
                {
                    String fromFile = origen;
                    String toFile = rutadestino + "/" + nombredestino;

                    //connect to server
                    sftp.Connect();

                    //subir archivo
                    sftp.Put(fromFile, toFile);

                    Console.WriteLine("archivo publicado");

                    //close connection
                    sftp.Close();
                    i = 5;
                    returnVal = 1;

                }

                catch (Exception ex)
                {
                    Thread.Sleep(3000);
                    Logger logger = LogManager.GetCurrentClassLogger();                    
                    logger = LogManager.GetCurrentClassLogger();                  

                    //close connection
                    
                    logger.Error(ex);
                    sftp.Close();


                }
            }
            return returnVal;
        }

    }
}
