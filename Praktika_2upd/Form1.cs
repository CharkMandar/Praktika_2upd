using OfficeOpenXml;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;


namespace Praktika_2upd
{
    public partial class Form1 : Form
    {

        #region === Properties ===
        bool _useArmed = false;

        Form Form2 = new Form();
        //System.Windows.Forms.ComboBox ComboBox1 = new System.Windows.Forms.ComboBox();
        System.Windows.Forms.TextBox TextBox1 = new System.Windows.Forms.TextBox();
        System.Windows.Forms.TextBox TextBox2 = new System.Windows.Forms.TextBox();
        System.Windows.Forms.TextBox TextBox3 = new System.Windows.Forms.TextBox();
        System.Windows.Forms.TextBox TextBox4 = new System.Windows.Forms.TextBox();
        Label Label1 = new Label();
        Label Label2 = new Label();
        Label Label3 = new Label();
        Label Label4 = new Label();
        System.Windows.Forms.Button Button3 = new System.Windows.Forms.Button();
        double r1, r2, r3, p2, p3, delta_r, heat, sigma_max;
        double E = 2000000;
        double alph = 0.0000125;
        double max_heat = 450;
        int n = 1000;
        double h;
        double[] sigma_inner;
        double[] sigma_outer;
        double[] sigma_summary;
        double[] sigma_homo;
        double[] radial_outer;
        double[] radial_summary;
        double[] radial_homo;
        double[] radial_inner;
        double[] r;

        int width, height;
        double x, y;
        string str;

        #endregion


        #region === Form ===
        public Form1()
        {
            InitializeComponent();
            sigma_homo = new double[n + 1];
            sigma_inner = new double[n + 1];
            sigma_outer = new double[n + 1];
            sigma_summary = new double[n + 1];
            radial_homo = new double[n + 1];
            radial_inner = new double[n + 1];
            radial_outer = new double[n + 1];
            radial_summary = new double[n + 1];
            r = new double[n + 1];

            //ComboBox1.Parent = this;
            //ComboBox1.Bounds = new Rectangle(0, 0, 100, 200);
            //ComboBox1.SelectedIndexChanged += new EventHandler(ComboBox1_SelectedIndexChanged);
            TextBox1.Parent = Form2;
            TextBox2.Parent = Form2;
            TextBox3.Parent = Form2;
            TextBox4.Parent = Form2;
            TextBox1.Bounds = new Rectangle(0, 20, 100, 20);
            TextBox2.Bounds = new Rectangle(0, 60, 100, 20);
            TextBox3.Bounds = new Rectangle(0, 100, 100, 20);
            TextBox4.Bounds = new Rectangle(0, 140, 100, 20);
            Label1.Parent = Form2;
            Label2.Parent = Form2;
            Label3.Parent = Form2;
            Label4.Parent = Form2;
            Label1.Bounds = new Rectangle(0, 5, 200, 20);
            Label2.Bounds = new Rectangle(0, 45, 200, 20);
            Label3.Bounds = new Rectangle(0, 85, 200, 20);
            Label4.Bounds = new Rectangle(0, 125, 200, 20);
            Label1.Text = "Модуль упругости";
            Label2.Text = "Коэфф. темп. расширения";
            Label3.Text = "Предел текучести";
            Label4.Text = "Предельная темп. нагрева";

            label11.Visible = false;
            label7.Visible = false;
            textBox4.Visible = false;
            textBox6.Visible = false;


            Form2.Bounds = new Rectangle(0, 0, 220, 220);
            Form2.StartPosition = FormStartPosition.CenterParent;

            chart1.Legends[0].Font = new Font("Arial", 14, FontStyle.Regular);
        }
        void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Form2.ShowDialog();
            str = TextBox1.Text;
            MessageBox.Show(str);
            str = "";
        }
        #endregion


        #region === Buttons ===
        private void button1_Click(object sender, EventArgs e)
        {


            if (_useArmed == false)
            {
                if (checkBox1.Checked == false && checkBox2.Checked == false && checkBox3.Checked == false && checkBox4.Checked == false)
                {
                    MessageBox.Show("Выберите больше пунктов!", "Внимание!");
                    return;
                }

                clear();
                get_params();
                selection_steel();


                if (checkBox3.Checked == true)
                {
                    int i = 0;
                    get_Summary();

                    while (i <= n)
                    {
                        if (i < get_ind() - 1)
                            this.chart1.Series[0].Points.AddXY(r[i], sigma_summary[i]);
                        if (i > get_ind() + 1)
                            this.chart1.Series[5].Points.AddXY(r[i], sigma_summary[i]);

                        else
                        {
                            this.chart1.Series[4].Points.AddXY(r[i], sigma_summary[i]);
                        }



                        i++;
                    }
                }

                if (checkBox1.Checked == true)
                {
                    int i = 0;
                    get_tention_for_homogen();
                    while (i <= n)
                    {
                        this.chart1.Series[2].Points.AddXY(r[i], sigma_homo[i]);
                        //MessageBox.Show(Convert.ToString(sigma_homo[i]));
                        i++;
                    }


                }

                if (checkBox2.Checked == true)
                {
                    int i = 0;
                    get_radial_homo();
                    while (i <= n)
                    {
                        this.chart1.Series[3].Points.AddXY(r[i], radial_homo[i]);
                        i++;
                    }
                }

                if (checkBox4.Checked == true)
                {
                    int i = 0;
                    get_radial_summary();
                    while (i <= n)
                    {
                        if (i <= get_ind() - 1)
                            this.chart1.Series[1].Points.AddXY(r[i], radial_summary[i]);
                        if (i > get_ind())
                            this.chart1.Series[7].Points.AddXY(r[i], radial_summary[i]);

                        else
                        {
                            this.chart1.Series[8].Points.AddXY(r[i], radial_summary[i]);
                        }
                        i++;
                    }
                }

                get_heat();


                if (ExcelToolStripMenuItem.Checked == true)
                    ExcelOut(sigma_inner, sigma_outer, sigma_homo, sigma_summary, r3, false);

                //MessageBox.Show(Convert.ToString(heat));
                dataGridView2.Rows.Add("Температура нагрева", heat);
                dataGridView2.Rows.Add("Макс. напряжение однородн.", sigma_homo[0]);
                dataGridView2.Rows.Add("Макс. напряжение сост. ", sigma_summary[get_ind()]);
                dataGridView2.Rows.Add("Макс. допустимое напр.", sigma_max);
                dataGridView2.Rows.Add("Мин. напряжение однородн", sigma_homo[n]);
                dataGridView2.Rows.Add("Мин. напряжение сост", sigma_summary[get_ind() - 2]);
            }

            else
            {
                //double innerR, double outerR, double Angl, int amount, double innerP, double Step
                ArmedThreads pipe = new ArmedThreads(Convert.ToDouble(textBox8.Text), Convert.ToDouble(textBox9.Text), Convert.ToDouble(textBox12.Text)
                    , Convert.ToInt32(textBox7.Text), Convert.ToDouble(textBox11.Text));
                h = (pipe.getR3() - pipe.getR2()) / Convert.ToDouble(n);

                r[0] = (pipe.getR2());
                for (int i = 1; i < n; i++)
                {
                    r[i] = (pipe.getR2() + (i * h));

                    //MessageBox.Show(Convert.ToString(r[i]));
                }
                r[n] = (pipe.getR3());

                double[] Inner = pipe.getTentionInPipe(r);
                double[] Outer = pipe.getTentionInCore(r);
                double[] Homo = pipe.getTentionInHomogen(r);
                double[] Summary = pipe.getSummaryTention(r, Homo, Inner, Outer);

                //for(int i = 0; i < r.Length + 1; i++)
                //    dataGridView1.Rows.Add(i, r[i], Summary[i], Homo[i], Outer[i], Inner[i]);


                //MessageBox.Show(Convert.ToString(r[0]));
                //MessageBox.Show(Convert.ToString(r[n]));

                if (checkBox3.Checked == true)
                {
                    int i = 0;
                    while (i < r.Length)
                    {
                        if (r[i] < pipe.getR1() - 1)
                            this.chart1.Series[0].Points.AddXY(r[i], Summary[i]);
                        else if (r[i] > pipe.getR1() + 1)
                        
                            this.chart1.Series[5].Points.AddXY(r[i], Summary[i]);
                        
                        else
                        {
                            this.chart1.Series[4].Points.AddXY(r[i], Summary[i]);
                        }


                        i++;
                    }
                }

                if (checkBox1.Checked == true)
                {
                    
                    int i = 0;
                    while (i < r.Length)
                    {
                        this.chart1.Series[2].Points.AddXY(r[i], Homo[i]);

                        i++;
                    }


                }

                if (checkBox2.Checked == true)
                {
                    Inner = pipe.getRadialInPipe(r);
                    Outer = pipe.getTentionInCore(r);
                    Homo = pipe.getRadialInHomogen(r);
                    Summary = pipe.getRadialSummary(r, Homo, Inner, Outer);

                    int i = 0;
                    while (i < r.Length)
                    {
                        this.chart1.Series[3].Points.AddXY(r[i], Homo[i]);

                        i++;
                    }
                }

                if (checkBox4.Checked == true)
                {
                    Inner = pipe.getRadialInPipe(r);
                    Outer = pipe.getTentionInCore(r);
                    Homo = pipe.getRadialInHomogen(r);
                    Summary = pipe.getRadialSummary(r, Homo, Inner, Outer);

                    int i = 0;
                    while (i < r.Length)
                    {
                        if (r[i] < pipe.getR1() - 1)
                            this.chart1.Series[1].Points.AddXY(r[i], Summary[i]);
                        else if (r[i] > pipe.getR1() + 1)

                            this.chart1.Series[7].Points.AddXY(r[i], Summary[i]);

                        else
                        {
                            this.chart1.Series[8].Points.AddXY(r[i], Summary[i]);
                        }


                        i++;
                    }
                }


                if (ExcelToolStripMenuItem.Checked == true)
                {
                    ExcelOut(Inner, Outer, Homo, Summary, pipe.getR1(), true);
                    //MessageBox.Show("Esxcel Complete!");
                }
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.chart1.Series[0].Points.Clear();
            this.chart1.Series[1].Points.Clear();
            this.chart1.Series[2].Points.Clear();
            this.chart1.Series[3].Points.Clear();
            this.chart1.Series[4].Points.Clear();
            this.chart1.Series[5].Points.Clear();
            this.chart1.Series[6].Points.Clear();
            this.chart1.Series[7].Points.Clear();
            this.chart1.Series[8].Points.Clear();
            dataGridView1.Rows.Clear();
            dataGridView2.Rows.Clear();

        }

        private void clear()
        {
            this.chart1.Series[0].Points.Clear();
            this.chart1.Series[1].Points.Clear();
            this.chart1.Series[2].Points.Clear();
            this.chart1.Series[3].Points.Clear();
            this.chart1.Series[4].Points.Clear();
            this.chart1.Series[5].Points.Clear();
            this.chart1.Series[6].Points.Clear();
            this.chart1.Series[7].Points.Clear();
            this.chart1.Series[8].Points.Clear();
            dataGridView1.Rows.Clear();
            dataGridView2.Rows.Clear();
        }

        #endregion


        #region === CalcRadial ===
        public void get_radial_inner()
        {
            for (int i = 0; i < get_ind(); i++)
            {
                radial_inner[i] = (-((p3 * r3 * r3) / (r3 * r3 - r2 * r2)) - ((p3 * r3 * r3 * r2 * r2) / (r[i] * r[i] * (r3 * r3 - r2 * r2))));
                //radial_inner[i] = (p2 * r2 * r2 - p3 * r3 * r3)/(r3 * r3 - r2 * r2) - ((p2 - p3) * r3 * r3 * r2 * r2) / ((r3 * r3 - r2 * r2) * r[i] * r[i]);
            }
        }

        public void get_radial_outer()
        {
            for (int i = get_ind(); i <= n; i++)
            {
                //radial_outer[i] = (p2 * r3 * r3 + p3 * r1 * r1) / (r1 * r1 - r3 * r3) - ((p2 + p3) * r3 * r3 * r1 * r1) / ((r1 * r1 - r3 * r3) * r[i] * r[i]);
                radial_outer[i] = (-((p3 * r3 * r3) / (r1 * r1 - r3 * r3)) + ((p3 * r3 * r3 * r1 * r1) / (r[i] * r[i] * (r1 * r1 - r3 * r3))));
            }
        }

        public void get_radial_homo()
        {
            for (int i = 0; i <= n; i++)
            {
                this.radial_homo[i] = ((p2 * r2 * r2 / (r1 * r1 - r2 * r2)) * (1 - (r1 * r1 / (r[i] * r[i]))));

            }
        }
        public void get_radial_summary()
        {
            get_radial_homo();
            get_radial_inner();
            get_radial_outer();


            for (int i = 0; i < n + 1; i++)
            {
                if (i < get_ind())
                {
                    this.radial_summary[i] = (radial_homo[i] + radial_inner[i]);
                    //radial_summary[i] = ((p2 * r2 * r2) / (r1 * r1 - r2 * r2)) * (1 - (r1 * r1) / (r[i] * r[i])) + ((p3 * r3 * r3) / (r1 * r1 - r3 * r3)) * (1 - (r1 * r1 / (r[i] * r[i])));
                }
                else if (i >= get_ind())
                {
                    this.radial_summary[i] = (radial_homo[i] - radial_outer[i]);
                    //radial_summary[i] = ((p2 * r2 * r2) / (r1 * r1 - r2 * r2)) * (1 - (r1 *- r1) / (r[i] * r[i])) + ((p3 * r3 * r3) / (r1 * r1 - r3 * r3)) * (1 - (r1 * r1 / (r[i] * r[i])));
                }
            }


        }
        #endregion


        #region === CalcTangent ===

        public void get_tention_for_inner(double p3)
        {
            for (int i = 0; i <= get_ind(); i++)
            {
                sigma_inner[i] = (-((p3 * r3 * r3) / (r3 * r3 - r2 * r2)) - ((p3 * r3 * r3 * r2 * r2) / (r[i] * r[i] * (r3 * r3 - r2 * r2))));
            }
        }

        public void get_tention_for_outer(double p3)
        {
            for (int i = get_ind(); i < n + 1; i++)
            {
                this.sigma_outer[i] = (((p3 * r3 * r3) / (r1 * r1 - r3 * r3)) + ((p3 * r3 * r3 * r1 * r1) / (r[i] * r[i] * (r1 * r1 - r3 * r3))));
            }
        }



        public void get_tention_for_homogen()
        {
            for (int i = 0; i < n + 1; i++)
            {
                this.sigma_homo[i] = (((p2 * r2 * r2) / (r1 * r1 - r2 * r2)) * (1 + ((r1 * r1) / (r[i] * r[i]))));
            }

        }


        private void get_Summary()
        {

            get_tention_for_inner(p3);
            get_tention_for_outer(p3);
            get_tention_for_homogen();


            for (int i = 0; i < n + 1; i++)
            {
                if (i < get_ind())
                {
                    this.sigma_summary[i] = (sigma_homo[i] + sigma_inner[i]);

                }
                else if (i >= get_ind())
                {
                    this.sigma_summary[i] = (sigma_homo[i] + sigma_outer[i]);
                }


                dataGridView1.Rows.Add(i, r[i], this.sigma_summary[i], this.sigma_homo[i], this.sigma_outer[i], this.sigma_inner[i], this.radial_homo[i], this.radial_summary[i]);
            }


        }
        #endregion


        #region === getStuff ===
        public int get_ind()
        {
            for (int i = 0; i <= n; ++i)
            {
                if ((int)r[i] == (int)r3)
                    return i;
            }
            return n / 2;
        }

        public void get_p3()
        {
            p3 = (delta_r * E * (r1 * r1 - r3 * r3) * (r3 * r3 - r2 * r2)) / (2 * r3 * r3 * r3 * (r1 * r1 - r2 * r2));
        }


        public void get_heat()
        {
            this.heat = (2 * p3 * r3 * r3 * (r1 * r1 - r2 * r2)) / (E * alph * (r1 * r1 - r3 * r3) * (r3 * r3 - r2 * r2));
        }

        public void get_delta_r()
        {
            delta_r = ((2 * p2 * r3) / E) * ((r1 - r2) / (r1 + r2));
        }

        //private void chart1_Click(object sender, System.Windows.Forms.MouseEventArgs e)
        //{
        //    // Получаем координаты курсора в пикселях
        //    int xPixel = e.X;
        //
        //    // Преобразуем пиксельные координаты в координаты графика
        //    double xValue = chart1.ChartAreas[0].AxisX.PixelPositionToValue(xPixel);
        //
        //    // Определяем область увеличения (настраивайте значения по необходимости)
        //    double zoomWidth = 2;  // Ширина области увеличения (меньше, чем раньше)
        //    double zoomHeight = 200; // Высота области увеличения
        //
        //    // Задаем новые границы так, чтобы увеличивалась область СПРАВА от точки клика.
        //    double newMinX = xValue; // Начало области увеличения - в точке клика
        //    double newMaxX = chart1.ChartAreas[0].AxisX.Maximum; // Конец области - максимальное значение оси X
        //    double newMinY = chart1.ChartAreas[0].AxisY.Minimum; // Минимум оси Y
        //    double newMaxY = chart1.ChartAreas[0].AxisY.Maximum; // Максимум оси Y
        //
        //
        //    // Устанавливаем новые границы для масштабирования, но ограничиваем zoomWidth
        //    chart1.ChartAreas[0].AxisX.ScaleView.Size = Math.Min(zoomWidth, newMaxX - newMinX); //Ограничение размера области
        //    chart1.ChartAreas[0].AxisX.ScaleView.Position = newMinX;
        //    chart1.ChartAreas[0].AxisY.ScaleView.Size = zoomHeight;
        //    chart1.ChartAreas[0].AxisY.ScaleView.Position = newMinY;
        //
        //    // Ограничиваем ScaleView, чтобы избежать выхода за пределы данных
        //    chart1.ChartAreas[0].AxisX.ScaleView.Position = Math.Max(chart1.ChartAreas[0].AxisX.Minimum, Math.Min(chart1.ChartAreas[0].AxisX.ScaleView.Position, chart1.ChartAreas[0].AxisX.Maximum - chart1.ChartAreas[0].AxisX.ScaleView.Size));
        //    chart1.ChartAreas[0].AxisY.ScaleView.Position = Math.Max(chart1.ChartAreas[0].AxisY.Minimum, Math.Min(chart1.ChartAreas[0].AxisY.ScaleView.Position, chart1.ChartAreas[0].AxisY.Maximum - chart1.ChartAreas[0].AxisY.ScaleView.Size));
        //
        //    // Перерисовка графика
        //    chart1.Invalidate();
        //}

        public void get_optimal()
        {
            r3 = Math.Sqrt(r2 * r1);
            p3 = (p2 * (r1 - r2)) / (2 * (r1 + r2));
        }

        public void get_params()
        {
            if (textBox1.Text != "" && textBox2.Text != "" && textBox3.Text != "" && checkBox5.Checked == false)
            {
                this.r2 = Convert.ToDouble(textBox1.Text);
                this.r1 = Convert.ToDouble(textBox2.Text);
                this.p2 = Convert.ToDouble(textBox3.Text);
                this.h = (r1 - r2) / Convert.ToDouble(n);
                r[0] = (r2);
                for (int i = 1; i < n; i++)
                {

                    r[i] = (r2 + (i * h));

                }
                r[n] = (r1);
                get_optimal();
                get_delta_r();

            }

            else if (textBox1.Text != "" && textBox2.Text != "" && textBox3.Text != "" && ((checkBox5.Checked == true && textBox4.Text == "") || (checkBox5.Checked == true && textBox6.Text == "")))
            {
                MessageBox.Show("Вы ввели не все расширенные параметры. Используются оптимальные соотношения", "Внимание!");
                this.r2 = Convert.ToDouble(textBox1.Text);
                this.r1 = Convert.ToDouble(textBox2.Text);
                this.p2 = Convert.ToDouble(textBox3.Text);
                this.h = (r1 - r2) / Convert.ToDouble(n);
                r[0] = (r2);
                for (int i = 1; i < n; i++)
                {

                    r[i] = (r2 + (i * h));

                }
                r[n] = (r1);
                get_optimal();
                get_delta_r();
            }
            else if (textBox1.Text != "" && textBox2.Text != "" && textBox3.Text != "" && checkBox5.Checked == true)
            {
                this.r2 = Convert.ToDouble(textBox1.Text);
                this.r1 = Convert.ToDouble(textBox2.Text);
                this.p2 = Convert.ToDouble(textBox3.Text);
                this.r3 = Convert.ToDouble(textBox4.Text);
                this.p3 = Convert.ToDouble(textBox6.Text);
                this.h = (r1 - r2) / Convert.ToDouble(n);

                r[0] = (r2);
                for (int i = 1; i < n; i++)
                {

                    r[i] = (r2 + (i * h));

                }
                r[n] = (r1);
                get_delta_r();

            }
            else if (textBox1.Text == "" && textBox2.Text == "" && textBox3.Text == "" && textBox4.Text == "")
            {
                this.r1 = 20;
                this.r2 = 10;
                this.r3 = 15;
                this.p2 = 3000;
                this.delta_r = 0.008;
                this.h = (r1 - r2) / Convert.ToDouble(n);

                r[0] = (r2);
                for (int i = 1; i < n; i++)
                {
                    r[i] = (r2 + (i * h));


                }
                r[n] = (r1);
                get_p3();

            }



            //MessageBox.Show(Convert.ToString(r[0]));
            //MessageBox.Show(Convert.ToString(r[n]));
            //MessageBox.Show(Convert.ToString(delta_r));
            //MessageBox.Show(Convert.ToString(p3));
        }

        public void selection_steel()
        {
            switch (comboBox1.Text)
            {
                case "12Х18Н10Т":
                    this.E = 1980000;
                    this.alph = 0.0000166;
                    this.max_heat = 600;
                    this.sigma_max = 5100;
                    break;

                case "09Г2С":
                    this.E = 2000000;
                    this.alph = 0.0000114;
                    this.max_heat = 450;
                    this.sigma_max = 3430;
                    break;

                case "20 пс":
                    this.E = 2120000;
                    this.alph = 0.0000123;
                    this.max_heat = 425;
                    this.sigma_max = 2450;
                    break;
                case "":
                    E = 2000000;
                    alph = 0.0000125;
                    max_heat = 450;
                    sigma_max = 2500;
                    break;
                case "Ручн. ввод":
                    Form2.ShowDialog();

                    if ((TextBox1.Text == "") || (TextBox2.Text == "") || (TextBox3.Text == "") || (TextBox4.Text == ""))
                    {
                        MessageBox.Show("Вы ввели не все параметры! Будут использованы значения по умолчанию!", "Внимание!");
                        E = 2000000;
                        alph = 0.0000125;
                        max_heat = 450;
                        sigma_max = 2500;

                        break;
                    }
                    E = Convert.ToDouble(TextBox1.Text);
                    alph = Convert.ToDouble(TextBox2.Text);
                    sigma_max = Convert.ToDouble(TextBox3.Text);
                    max_heat = Convert.ToDouble(TextBox4.Text);

                    break;
            }
        }

        #endregion


        #region === TopPanel ===
        private void toolStripButton1_Click_1(object sender, EventArgs e)
        {
            Process.Start("Help\\Main.html");
        }

        private void armedToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            groupBox3.Visible = false;
            groupBox4.Visible = true;
            armedToolStripMenuItem.Checked = true;
            thickToolStripMenuItem.Checked = false;
            _useArmed = true;
        }
        private void thickToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            groupBox3.Visible = true;
            groupBox4.Visible = false;
            armedToolStripMenuItem.Checked = false;
            thickToolStripMenuItem.Checked = true;
            _useArmed = false;
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox5.Checked == true)
            {

                label11.Visible = true;
                label7.Visible = true;
                textBox4.Visible = true;
                textBox6.Visible = true;
            }
            else
            {
                label11.Visible = false;
                label7.Visible = false;
                textBox4.Visible = false;
                textBox6.Visible = false;
            }
        }

        private void ExcelToolStripMenuItem_Click(object sender, EventArgs e) 
        {
            if (ExcelToolStripMenuItem.Checked == false)
                ExcelToolStripMenuItem.Checked = true;
            else
                ExcelToolStripMenuItem.Checked = false; 
        }

        private void ExcelOut(double[] inner, double[] outer, double[] homo, double[] summary, double r3, bool Armed)
        {
            string path;
            if (Armed == true)
                path = @"D:\ArmedData.xlsx";
            else
                path = @"D:\PipeData.xlsx";

            FileInfo newFile = new FileInfo(path);
            if (newFile.Exists)
            {
                newFile.Delete();
                newFile = new FileInfo(path);
            }
            using (ExcelPackage package = new ExcelPackage(newFile))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Значения");

                worksheet.Cells.Style.WrapText = true;   
                worksheet.Cells.Style.Font.Bold = true;
                worksheet.Cells[1, 1].Value = "X";
                worksheet.Cells[1, 2].Value = "Однородная";
                worksheet.Cells[1, 3].Value = "Внутренняя";
                worksheet.Cells[1, 4].Value = "Внешняя";
                worksheet.Cells[1, 5].Value = "Составня";
                worksheet.Cells.Style.Font.Bold = false;

                for(int i = 2; i < r.Length+2; i++)
                {
                    worksheet.Cells[i, 1].Value = r[i - 2];
                    worksheet.Cells[i, 2].Value = homo[i - 2];
                    worksheet.Cells[i, 5].Value = summary[i - 2];
                    
                    if(r[i-2] < r3)
                        worksheet.Cells[i, 3].Value = inner[i - 2];
                    else
                        worksheet.Cells[i, 4].Value = outer[i - 2];
                }

                package.Save();
            }
        }
        #endregion

    }


    #region Мусор 
    // private void Button3_Click(object sender, EventArgs e)
    // {
    //     if ((TextBox1.Text == "") || (TextBox2.Text == "") || (TextBox3.Text == "") || (TextBox4.Text == ""))
    //     {
    //         MessageBox.Show("Вы ввели не все параметры! Будут использованы значения по умолчанию!", "Внимание!");
    //         E = 2000000;
    //         alph = 0.0000125;
    //         max_heat = 450;
    //         sigma_max = 2500;
    //     }
    //    // MessageBox.Show("Кнопка отработала");
    //     Form2.Hide();
    //     
    // }


    //void selection_dispertion()
    //{
    //    if (textBox5.Text == "")
    //        n = 1000;
    //    else
    //    {
    //        n = Convert.ToInt32(textBox5.Text);
    //    }
    //}
    //public void selection_params(double r1, double p2)
    //{
    //    this.r1 = r1;
    //    this.p2 = p2;
    //    this.h = 1 / n;
    //    this.r2 = r1 + h;
    //    int i = 1;
    //    while (true)
    //    {
    //
    //        get_optimal();
    //        get_Summary();
    //        get_heat();
    //        if (heat > 450)
    //        {
    //            r2 += i * h;
    //            i++;
    //        }
    //        else
    //        {
    //            break;
    //        }
    //    }
    //
    //
    //}
    #endregion
}
