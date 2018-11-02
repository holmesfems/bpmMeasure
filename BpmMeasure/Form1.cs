using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Distributions;


namespace BpmMeasure
{
    public partial class Form1 : Form
    {
        Color btnDefaultColor;
        List<TimeSpan> dtList = new List<TimeSpan>();
        DateTime FirstClick;

        public Form1()
        {
            InitializeComponent();
            btnDefaultColor = button3.BackColor;
        }
        
        private List<double> getBPM(List<TimeSpan> dtl)
        {
            int len = dtl.Count;
            if(len < 2)
            {
                return new List<double>();
            }
            if(len == 2)
            {
                double bpm = 60 / (dtl[1].TotalSeconds - dtl[0].TotalSeconds);
                return new List<double>
                {
                    bpm,
                    0
                };
            }
            var Mat = DenseMatrix.OfArray(new double[len, len]);
            for (int i = 0; i < len; i++)
            {
                Mat[0, i] = 1;
                Mat[1, i] = i;
                if (i >= 2) Mat[i, i] = 1;
            }
            for (int i = 2; i < len; i++)
            {
                Mat[i, 0] = i - 1;
                Mat[i, 1] = -i;
            }
            var Y = DenseVector.OfArray(new double[len]);
            Y[1] = 1;
            var b = Mat.Solve(Y);
            var dtVec = DenseVector.OfArray(new double[len]);
            for (int i = 0; i < len; i++)
            {
                dtVec[i] = dtl[i].TotalSeconds;
            }
            double dt = b * dtVec;
            double s = b * b;
            for (int i = 0; i < len; i++)
            {
                dtVec[i] -= i * dt;
            }
            double dtVar = Math.Sqrt(dtVec * dtVec / (len - 1) * s);

            //Console.WriteLine(b.ToString());
            return new List<double> { 60 / dt, 3 * dtVar * 60 / (dt * dt) };
        }
          
        private void button1_Click(object sender, EventArgs e)
        {
            if(dtList.Count == 0)
            {
                FirstClick = DateTime.Now;
                dtList.Add(TimeSpan.Zero);
            }
            else
            {
                dtList.Add(DateTime.Now - FirstClick);
            }
            var res = getBPM(dtList);
            if (res.Count > 0)
            {
                textBox3.Text = Convert.ToString(res[0]);
                textBox4.Text = Convert.ToString(res[1]);
            }
            textBox1.Text = Convert.ToString(dtList.Count);
            textBox2.Text = Convert.ToString(dtList.Last().TotalSeconds);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            dtList.Clear();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            textBox5.Text = textBox3.Text;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!timer1.Enabled)
            {
                try
                {
                    timer1.Interval = Convert.ToInt32(60000 / Convert.ToDouble(textBox5.Text));
                    button3.Text = "Stop (&S)";
                    timer1.Enabled = true;
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }
            else
            {
                timer1.Enabled = false;
                button3.Text = "Shine (&S)";
                button3.BackColor = btnDefaultColor;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Color col = button3.BackColor;
            if (col.Equals(btnDefaultColor))
            {
                button3.BackColor = Color.Red;
            }
            else
            {
                button3.BackColor = btnDefaultColor;
            }
        }
    }
}
