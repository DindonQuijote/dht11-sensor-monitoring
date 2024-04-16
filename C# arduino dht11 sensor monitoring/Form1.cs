using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace C__arduino_dht11_sensor_monitoring
{
    public partial class Form1 : Form
    {
        double temperature=0, humidity=0;
        bool updateData = false;
        private const int MaxDataPoints = 25;
        DateTime startTime; //new
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button_open.Enabled = true;
            button_close.Enabled = false;
            chart1.Series["Temperature"].Points.AddXY(1, 1);
           chart1.ChartAreas["ChartArea1"].AxisX.Title = "Time [s]";
            chart1.ChartAreas["ChartArea1"].AxisY.Title = "Temperature [°C]";
            chart2.Series["Humidity"].Points.AddXY(1, 1);
            chart2.ChartAreas["ChartArea1"].AxisX.Title = "Time [s]";
            chart2.ChartAreas["ChartArea1"].AxisY.Title = "Humidity [%]";
        }

        private void comboBox_portlist_DropDown(object sender, EventArgs e)
        {
            string[] portLists = SerialPort.GetPortNames();
            comboBox_portlist.Items.Clear();
            comboBox_portlist.Items.AddRange(portLists);
        }

        private void button_open_Click(object sender, EventArgs e)
        {
            try {
                serialPort1.PortName = comboBox_portlist.Text;
                
                serialPort1.BaudRate = Convert.ToInt32(comboBox_baudRate.Text);
                serialPort1.Open();
                button_open.Enabled = false;
                button_close.Enabled = true;
                chart1.Series["Temperature"].Points.Clear();
                chart2.Series["Humidity"].Points.Clear();

                MessageBox.Show("Success connected to Arduino board");

            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);

            }
        }

        private void button_close_Click(object sender, EventArgs e)
        {
            try
            {
                
                serialPort1.Close();
                button_open.Enabled = true;
                button_close.Enabled = false;

                MessageBox.Show("Disconnected to Arduino board");

            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);

            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {

                serialPort1.Close();

            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);

            }
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string dataIn = serialPort1.ReadTo("\n");
            Data_Parsing(dataIn);
            /// new
            if (!updateData) {
                startTime = DateTime.Now;
            }
            //updateData = true; //????
            /// 

            this.BeginInvoke(new EventHandler(Shaw_Data));
            Console.WriteLine(dataIn);
        }

        private void Shaw_Data(object sender, EventArgs e)
        {
            if (updateData)
            {
                TimeSpan elapsedTime = DateTime.Now - startTime;//new
                DateTime currentTime = DateTime.Now;
                // Обновляем элементы управления интерфейса из главного потока
                this.Invoke((MethodInvoker)(() =>
                {
                    label_temperature.Text = string.Format("Temperature {0} [°C]", temperature.ToString());
                    label_humidity.Text = string.Format("Humidity {0} [%]", humidity.ToString());
                    int sexonds = (int)Math.Round(elapsedTime.TotalSeconds);
                    chart1.Series["Temperature"].Points.AddXY(sexonds, temperature);
                    
                    chart2.Series["Humidity"].Points.AddXY(sexonds, humidity);

                    while (chart1.Series["Temperature"].Points.Count > MaxDataPoints) {
                        chart1.Series["Temperature"].Points.RemoveAt(0);
                        chart2.Series["Humidity"].Points.RemoveAt(0);
                    }


                }));
            }
        }

        private void button_export_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                //export data
                ExportToCSV(saveFileDialog.FileName);
            }
        }

        private void ExportToCSV(string filePath)
        {
            try {
                //print names
                using(StreamWriter sw = new StreamWriter(filePath,false, Encoding.UTF8)) {
                    sw.WriteLine("Time, Temperature, Humidity");

                    //printing data
                    for (int i = 0; i < chart1.Series["Temperature"].Points.Count; i++) { 
                        string time = chart1.Series["Temperature"].Points[i].XValue.ToString();
                        string temperature = chart1.Series["Temperature"].Points[i].YValues[0].ToString();
                        string humidity = chart2.Series["Humidity"].Points[i].YValues[0].ToString();

                        sw.WriteLine($"{time}, {temperature}, {humidity}");
                    }
                
                }
                MessageBox.Show($"Export is succesfully made", " FINAL ", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex) {

                MessageBox.Show($"Error during data export: {ex.Message}", " Error ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            
            }
        }

        private void button_save_Click(object sender, EventArgs e)
        {
            
        }

        private void Data_Parsing(string data) {
            sbyte indexOf_startDataCharacter = (sbyte)data.IndexOf("@");
            sbyte indexOf_A = (sbyte)data.IndexOf("A");
            sbyte indexOf_B = (sbyte)data.IndexOf("B");
            // if charachtres "A", "B" and "@" exist in the data Package 
            if (indexOf_A != -1 && indexOf_B != -1 && indexOf_startDataCharacter != -1)
            {
                try
                {
                    string str_temperature = data.Substring(indexOf_startDataCharacter + 1, (indexOf_A - indexOf_startDataCharacter) - 1);
                    string str_humidity = data.Substring(indexOf_A + 1, (indexOf_B - indexOf_A) - 1);

                    temperature = Convert.ToDouble(str_temperature, System.Globalization.CultureInfo.InvariantCulture);
                    humidity = Convert.ToDouble(str_humidity, System.Globalization.CultureInfo.InvariantCulture);

                    

                    updateData = true;

                }
                catch (Exception)
                {

                }


            }
            else {
                updateData = false;
            }
        }
    }
}
