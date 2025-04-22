using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Praktika_2upd
{
    internal class Pipe
    {
        double[] sigma_inner;
        double[] sigma_outer;
        double[] sigma_summary;
        double[] sigma_homo;
        double[] radial_outer;
        double[] radial_summary;
        double[] radial_homo;
        double[] radial_inner;
        double[] r;
        double r1, r2, r3, p2, p3, delta_r, heat, sigma_max;
        double E = 2000000;
        double alph = 0.0000125;
        double max_heat = 450;
        int n = 1000;
        double h;
        public Pipe()
        {
            sigma_homo = new double[n + 1];
            sigma_inner = new double[n + 1];
            sigma_outer = new double[n + 1];
            sigma_summary = new double[n + 1];
            radial_homo = new double[n + 1];
            radial_inner = new double[n + 1];
            radial_outer = new double[n + 1];
            radial_summary = new double[n + 1];
            r = new double[n + 1];
        }

        #region === CalcRadial ===
        public void get_radial_inner()
        {
            for (int i = 0; i < get_ind(); i++)
            {
                radial_inner[i] = (-((p3 * r3 * r3) / (r3 * r3 - r2 * r2)) - ((p3 * r3 * r3 * r2 * r2) / (r[i] * r[i] * (r3 * r3 - r2 * r2))));
                
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

        public void get_Summary()
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


                //dataGridView1.Rows.Add(i, r[i], this.sigma_summary[i], this.sigma_homo[i], this.sigma_outer[i], this.sigma_inner[i], this.radial_homo[i], this.radial_summary[i]);
            }


        }
        #endregion

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

        public void get_optimal()
        {
            r3 = Math.Sqrt(r2 * r1);
            p3 = (p2 * (r1 - r2)) / (2 * (r1 + r2));
        }

    }
}
