using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using _32_1_Ivanova_project1.NeuroNet;
using System.IO;

namespace _32_1_Ivanova_project1
{
    public partial class FormMain : Form
    {
        private double[] inputPixels = new double[16];
        private Network network;
        public FormMain()
        {
            InitializeComponent();
            inputPixels = new double[15];
            network = new Network();
        }


        private void recognizeButton_Click(object sender, EventArgs e)
        {
            network.ForwardPass(inputPixels);

            labelOutput.Text = network.Fact.ToList().IndexOf(network.Fact.Max()).ToString();
            labelProbability.Text = (100 * network.Fact.Max()).ToString("0.00") + " %";

        }

        private void Changing_Status_button(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int index = int.Parse(btn.Name.Replace("button", "")) - 1;

            if (index >= 0 && index < inputPixels.Length)
            {
                if (btn.BackColor == Color.White)
                {
                    btn.BackColor = Color.Black;
                    inputPixels[index] = 1d;
                }
                else
                {
                    btn.BackColor = Color.White;
                    inputPixels[index] = 0d;
                }
            }
        }

        private void button_SaveTrainSample_Click(object sender, EventArgs e)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "train.txt";
            string tmpStr = numericUpDown_NecessaryOutput.Value.ToString();

            for (int i = 0; i < inputPixels.Length; i++)
            {
                tmpStr += " " + inputPixels[i].ToString();
            }
            tmpStr += "\n";

            File.AppendAllText(path, tmpStr); 
        }

        private void button_SaveTestSample_Click(object sender, EventArgs e)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "test.txt";
            string tmpStr = numericUpDown_NecessaryOutput.Value.ToString();

            for (int i = 0; i < inputPixels.Length; i++)
            {
                tmpStr += " " + inputPixels[i].ToString();
            }
            tmpStr += "\n";

            File.AppendAllText(path, tmpStr);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
