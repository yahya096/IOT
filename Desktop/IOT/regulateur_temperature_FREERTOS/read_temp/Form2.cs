using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using ExcelDataReader;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.IO.Packaging;
using System.Runtime.CompilerServices;
using System.Windows.Forms.DataVisualization.Charting;
using static OfficeOpenXml.ExcelErrorValue;
using Microsoft.VisualBasic.ApplicationServices;
using System.Media;
using System.Numerics;
using System.Diagnostics;
using System.IO.Ports;
using System.Management;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;
using OfficeOpenXml.Style.XmlAccess;


namespace read_temp
{
    
    public partial class Form2 : Form

    {

        SerialPort serialPort=new SerialPort();
        private Excel.Application excelApp;
        private Excel.Workbook excelWB;
        private Excel._Worksheet excelWS;
        public int i,j=0;
        public double tempToProgressBar;
        double moyenneY = 0;
        double S_Temp_read = 0;
        double temp;


        public Form2()
        {
            

            InitializeComponent();
            InitializeSerialPortComboBox();
            comboBox2.SelectedIndexChanged += ComboBoxPorts_SelectedIndexChanged;
            serialPort.DataReceived += SerialPort_DataReceived;
            this.MaximizeBox = false;

            AddRoundedCorners(panel1, 40);
            AddRoundedCorners(panel2, 40);
            AddRoundedCorners(panel3, 40);          
            AddRoundedCorners(panel4, 40);


            excelApp = new Excel.Application();
            excelWB = excelApp.Workbooks.Add(Type.Missing);
            excelWS = (Excel._Worksheet)excelWB.ActiveSheet;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Êtes-vous sûr de vouloir quitter ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    serialPort.Close();
                }

                excelWB.SaveAs(@"C:\Users\Lenovo\Desktop\IOT\read_temp\read_temp\data.xlsx");
                excelWB.Close();
                excelApp.Quit();

                this.Hide();
                Form1 f1 = new Form1(); ;
                f1.ShowDialog();
            }


        }
        /****************  Panel esthetique ************************/
        private void AddRoundedCorners(Panel panel, int cornerRadius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, cornerRadius, cornerRadius, 180, 90);
            path.AddArc(panel.Width - cornerRadius, 0, cornerRadius, cornerRadius, 270, 90);
            path.AddArc(panel.Width - cornerRadius, panel.Height - cornerRadius, cornerRadius, cornerRadius, 0, 90);
            path.AddArc(0, panel.Height - cornerRadius, cornerRadius, cornerRadius, 90, 90);
            panel.Region = new Region(path);
        }

        private DataTable ReadExcelFile(string filePath)
        {
            DataTable dataTable = new DataTable();
            var enc = CodePagesEncodingProvider.Instance.GetEncoding(1252);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            // Utilisez ExcelDataReader pour lire le fichier Excel
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    // Déterminez le nombre de colonnes (supposez que la première ligne contient des en-têtes)
                    reader.Read();
                    int columnCount = reader.FieldCount;

                    // Ajoutez des colonnes à la DataTable
                    for (int i = 0; i < columnCount; i++)
                    {
                        dataTable.Columns.Add(reader.GetValue(i).ToString());
                    }

                    // Ajoutez les lignes à la DataTable
                    while (reader.Read())
                    {
                        DataRow row = dataTable.NewRow();
                        for (int i = 0; i < columnCount; i++)
                        {
                            row[i] = reader.GetValue(i);
                        }
                        dataTable.Rows.Add(row);
                    }
                }
            }

            return dataTable;
        }

        static int MapVPB(double X, double In_min, float In_max, float Out_min, float Out_max)
        {
            double A, B;
            A = X - In_min;
            B = Out_max - Out_min;
            A = A * B;
            B = In_max - In_min;
            A = A / B;
            return (int)(A + Out_min);
        }

        private void InitializeSerialPortComboBox()
        {
            // Ajoute les ports disponibles à la ComboBox
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                comboBox2.Items.Add(port);
            }
        }
        
        private void ComboBoxPorts_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedPort = comboBox2.SelectedItem as string;
            if (!string.IsNullOrEmpty(selectedPort))
            {

                serialPort.PortName= selectedPort;
                serialPort.BaudRate = 9600;
                serialPort.Open();

                MessageBox.Show("Port série ouvert avec succès!");

            }
            
           
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
           
            string receivedData = serialPort.ReadLine();

            if (receivedData.Contains(",") && receivedData.Length==5) // I choose 40 as a maximal temperature 
            {
                i++; // for updating X axis of the chart each interruption

                Invoke(new Action(() => textBox1.Text = (receivedData)));


                /************           update progressBar        ***********/

                Invoke(new Action(() => tempToProgressBar = Convert.ToDouble(receivedData)));
                int vpb_sy, vpb_ly;
                vpb_sy = MapVPB(tempToProgressBar, 0, 40, 0, 145);
                if (vpb_sy > 145)
                {
                    vpb_sy = 145;
                }
                if (vpb_sy < 0)
                {
                    vpb_sy = 0;
                }
                pictureBox2.Height = pictureBox1.Height * vpb_sy / 145;
                vpb_ly = (pictureBox1.Height - vpb_sy) + pictureBox1.Location.Y;
                pictureBox2.Location = new Point(pictureBox2.Location.X, vpb_ly);

                /************  END of updating progressBar ************/

                /************ Drawing chart in real time **********/

                chart1.Series[0].Points.AddXY(i, Convert.ToDouble(receivedData));

                /***********  Storage Values read by thermoeter in an excel file **************/

                Invoke(new Action(() => excelWS.Cells[i, 1] = receivedData));

                /******* DRAW in histogram the average temperature per day ***********/

                S_Temp_read += Convert.ToDouble(receivedData);
                moyenneY = S_Temp_read / i;
                chart2.Series[0].Points.Clear();
                chart2.Series[0].Points.AddXY(1, moyenneY);

                /********* FAN STATE ***********/

                if (Convert.ToDouble(receivedData) > 27)
                {
                    label9.BackColor = Color.Green;
                }
                else
                {
                    label9.BackColor = Color.Red;
                }

                /****** ACTIVATE alarm *****/
                SoundPlayer player = new SoundPlayer(@"C:\Users\Lenovo\Desktop\IOT\read_temp\read_temp\alarme.wav");

                Invoke(new Action(() => temp = Convert.ToDouble(receivedData)));
                if (temp > 27 && j == 0)
                {
                    j++;
                    player.Play();
                }
                else if (temp < 27)
                {
                    j = 0;
                    player.Stop();
                }
            }
            else { 
                //serialPort.Close();
            }
           
        }
       
        private void button3_Click(object sender, EventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
            }
         
            excelWB.SaveAs(@"C:\Users\Lenovo\Desktop\IOT\read_temp\read_temp\data.xlsx");      
            excelWB.Close();
            excelApp.Quit();

            //this.Hide();
            string processName = "read_temp";
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length > 0)
            {
                // Terminez tous les processus trouvés
                foreach (Process process in processes)
                {
                    process.Kill();
                }

                
            }

        }

    }

   
}
