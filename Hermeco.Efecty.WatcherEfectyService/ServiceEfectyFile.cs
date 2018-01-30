using Hermeco.Efecty.WatcherEfectyService.Utiles;
using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;


namespace Hermeco.Efecty.WatcherEfectyService
{
    public partial class ServiceEfectyFile : ServiceBase
    {
        Logger logger = LogManager.GetCurrentClassLogger();
        Queue myQ;

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public ServiceEfectyFile()
        {
            InitializeComponent();

            fileSystemWatcherEfecty.Created += new FileSystemEventHandler(fileSystemWatcherEfecty_Created);
            fileSystemWatcherEfecty.EnableRaisingEvents = true;
            fileSystemWatcherEfecty.Path = ConfigurationManager.AppSettings["WatchPath"];
            fileSystemWatcherEfecty.IncludeSubdirectories = false;
            fileSystemWatcherEfecty.NotifyFilter = NotifyFilters.LastWrite
           | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.CreationTime;
            fileSystemWatcherEfecty.Filter = "*xml";
        }

        protected override void OnStart(string[] args)
        {
            logger.Info("Inicio del servicio.");
            myQ = new Queue();
        }

        protected override void OnStop()
        {
            fileSystemWatcherEfecty.EnableRaisingEvents = false;
            fileSystemWatcherEfecty.Dispose();
            logger.Info("El servicio se ha detenido.");
        }

        private void fileSystemWatcherEfecty_Created(object sender, System.IO.FileSystemEventArgs e)
        {
            try
            {
                myQ.Enqueue(e.FullPath);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            Task.Factory.StartNew(ProcessQueue);  
            logger.Info("Procesando archivo " + e.Name);
            logger.Info("Cola " + myQ.Count);
            Thread process = new Thread(() => processCreatedFile(e));
            process.Start();
        }

        void ProcessQueue()
        {
            try
            {
                foreach (var file in myQ)
                {                     
                    readXML(myQ.Dequeue().ToString());
                    logger.Info("Cola " + myQ.Count);
                }
            }
            catch (Exception exception)
            {

            }
        }

        private void processCreatedFile(System.IO.FileSystemEventArgs e)
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    readXML(e.FullPath);
                    i = 5;
                }
                catch (IOException ex)
                {
                    Thread.Sleep(3000);
                    logger = LogManager.GetCurrentClassLogger();
                    logger.Error(ex, string.Format("Error al leer el archivo XML {0}", e.FullPath));
                }
                catch (XmlException ex)
                {
                    logger = LogManager.GetCurrentClassLogger();
                    logger.Error(ex, string.Format("Error al leer el archivo XML, la estructura no es correcta para el archivo {0}.", e.FullPath));
                    i = 5;
                }
                catch (Exception ex)
                {
                    logger = LogManager.GetCurrentClassLogger();
                    logger.Error(ex, string.Format("Error al procesar XML para el archivo {0}", e.FullPath));
                    i = 5;
                }
            }

        }

        private void readXML(string path)
        {
            Thread.Sleep(2000);
            var appSettings = ConfigurationManager.AppSettings;

            string pathEfectyFiles = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + appSettings["PathEfectyFiles"].ToString();

            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(path);

            XmlNodeList Etiquetas = xDoc.GetElementsByTagName("ETIQUETAS");

            foreach (XmlElement etiqueta in Etiquetas)
            {
                XmlNodeList items = etiqueta.GetElementsByTagName("item");

                string fileName = string.Format("FTP{0}.txt", DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                string guias = string.Empty;
                int countLines = 0;
                decimal valorTotal = 0;

                foreach (XmlElement item in items)
                {

                    XmlNodeList nOrgVentas = item.GetElementsByTagName("ORGVENTAS");
                    XmlNodeList nCanalDist = item.GetElementsByTagName("CANALDISTRI");
                    XmlNodeList nCondPago = item.GetElementsByTagName("CONDICIONPAGO");

                    if (nOrgVentas[0].InnerText.Equals("1030") && nCanalDist[0].InnerText.Equals("10") && nCondPago[0].InnerText.Equals("D01"))
                    {
                        XmlNodeList nNumeroGuia = item.GetElementsByTagName("NUMEROGUIA");
                        string guia = nNumeroGuia[0].InnerText.TrimStart('0'); //El número de guía no debe tener ningún cero adelante
                        XmlNodeList nValorRecaudo = item.GetElementsByTagName("VALORRECAUDO");
                        string fechaVencimiento = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        XmlNodeList nPedidoClienteC1 = item.GetElementsByTagName("PEDIDO_CLIENTE");
                        XmlNodeList nNombresApellidosC2 = item.GetElementsByTagName("NOMBREDESTINATARIO");
                        //XmlNodeList nApellidosC3 = item.GetElementsByTagName("NOMBREDESTINATARIO");
                        XmlNodeList nPedidoSAPC4 = item.GetElementsByTagName("PEDIDO_SAP");
                        XmlNodeList nCedula = item.GetElementsByTagName("CEDULA");
                        const string tipoIdentificacionC5 = "CEDULA DE CIUDADANIA";

                        guias += guia;

                        if (!string.IsNullOrEmpty(nValorRecaudo[0].InnerText) && decimal.Parse(nValorRecaudo[0].InnerText) != 0)
                        {
                            int valorRecaudo = Convert.ToInt32(decimal.Parse(nValorRecaudo[0].InnerText));

                            using (StreamWriter writer = new StreamWriter(string.Format("{0}\\{1}", pathEfectyFiles, fileName), true))
                            {
                                writer.WriteLine("\"01\"|REFERENCIA|VR OBLIGACION|FECHA VENCIMIENTO|CLAVE 1|CLAVE 2|CLAVE 3|CLAVE 4|CLAVE 5");
                                writer.WriteLine(string.Format("\"02\"|\"{0}\"|{1}|{2}|{3}|\"{4}\"|\"{5}\"|\"{6}\"|\"{7}\"", guia, valorRecaudo, fechaVencimiento, nPedidoClienteC1[0].InnerText, nNombresApellidosC2[0].InnerText.Length > 50 ? nNombresApellidosC2[0].InnerText.Substring(0, 50) : nNombresApellidosC2[0].InnerText, nPedidoSAPC4[0].InnerText, nCedula[0].InnerText, tipoIdentificacionC5));
                                countLines++;
                                valorTotal += Convert.ToDecimal(nValorRecaudo[0].InnerText);
                                writer.Close();
                            }
                        }
                    }
                }

                if (countLines > 0)
                {
                    using (StreamWriter writer = new StreamWriter(string.Format("{0}\\{1}", pathEfectyFiles, fileName), true))
                    {
                        writer.WriteLine(string.Format("\"03\"|{0}|{1}|{2}|\"\"|\"\"|\"\"|\"\"|\"\"", countLines, valorTotal, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                        writer.Close();
                    }

                    logger.Info(string.Format("Se ha generado el archivo {0}. Guías: {1}", fileName, guias));

                    int uploadFile = SFTP.UploadFile(appSettings["SFTPserver"], appSettings["SFTPuser"], appSettings["SFTPpassword"], pathEfectyFiles + fileName, appSettings["SFTPruta"], fileName);
                    if (uploadFile == 1)
                    {
                        logger.Info(string.Format("El archivo {0} fue cargado en el SFTP", fileName));
                    }
                }

            }
        }
    }
}
