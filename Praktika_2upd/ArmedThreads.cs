using Org.BouncyCastle.Crypto.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Praktika_2upd
{
    internal class ArmedThreads
    {

        #region === Properties ===
        double r2, r1, r3, Ang, P2, P3, L;
        int n;
        const double ThreadThickness = 0.1; // мм
        const double Sigma_max = 1.018; // МПа
        const double LD = 115;
        const double ro = 1.44;
        public ArmedThreads(double innerR, double outerR, double Step, int amount, double innerP) 
        {
            n = amount; // количество слоев
            r2 = innerR; // внутренний радиус - мм
            r3 = outerR + (ThreadThickness * n);// радиус с учетом намотки - мм 
            r1 = outerR; //внешний радиус ТРУБЫ - мм
            P2 = innerP; // внутреннее давление МПа
            L = Step;//2*Math.PI*r3/Math.Tan(Ang); // шаг - мм
            P3 = (P2 * (r1 - r2)) / (2 * (r1 + r2));
            Ang = Math.Atan(Math.PI * ThreadThickness / L);// угол намотки
        }

        public ArmedThreads()
        {
            r2 = 1;
            r1 = 2;
            r3 = 1.5;
            Ang = 0;
            n = 1;
            P2 = 1;
            L = 1;
        }
        #endregion

        #region === TentionFuncs ===
        public double[] getTentionInCore(double[] r)
        {
            double[] coreTention = new double[r.Length];
            for (int i = 0; i < r.Length; i++)
            {
                //coreTention[i] = (P3 * r[i] * L ) / (n * Math.Sin(Ang) * ThreadThickness * ThreadThickness );
                coreTention[i] = (n * Math.Sin(Ang) * ThreadThickness * ThreadThickness * P3 * 3800) / (r[i] * L);
            }
            return coreTention;
        }

        public double getPressureInCore()
        {
            return (n * Math.Sin(Ang) * ThreadThickness * ThreadThickness * Sigma_max) / (r3* L * 1000);
        }

        public double[] getTentionInPipe(double[] r)
        {
            double[] inner = new double[r.Length];
            double P1 = getPressureInCore();
            //inner[i] = (-((P3 * r3 * r3) / (r3 * r3 - r2 * r2)) - ((P3 * r3 * r3 * r2 * r2) / (r[i] * r[i] * (r3 * r3 - r2 * r2))));
            for(int i = 0; i < r.Length; i++)
                inner[i] = (-((P3 * r1 * r1) / (r1 * r1 - r2 * r2)) - ((P3 * r1 * r1 * r2 * r2) / (r[i] * r[i] * (r1 * r1 - r2 * r2))));
            //inner[i] = (P2 * r2 * r2 - P1 * r1 * r1) / (r1 * r1 - r2 * r2) + (P2 - P1) * r1 * r1 * r2 * r2 / (r[i] * r[i] * (r1 * r1 - r2 * r2));

            return inner;
        }

        

        public double[] getTentionInHomogen(double[] r )
        {
            double[] homoTention = new double[r.Length];
            for (int i = 0; i < r.Length; i++)
            {
                homoTention[i] = ((P2 * r2 * r2 / (r3 * r3 - r2 * r2)) * (1 + (r3 * r3 / (r[i] * r[i]))));
               // (((p2 * r2 * r2) / (r1 * r1 - r2 * r2)) * (1 + ((r1 * r1) / (r[i] * r[i]))));
            }

            return homoTention;
        }


        public double[] getSummaryTention(double[] r, double[] Homo, double[] Inner, double[] Outer)
        {
            double[] Summary = new double[r.Length];
            for (int i = 0; i < r.Length; i++)
            {
                if (r[i] < r1)
                {
                    Summary[i] = (Homo[i] + Inner[i]);
        
                }
                else if (r[i] >= r1)
                {
                    Summary[i] = (Homo[i] + Outer[i]);
                }
        
            }
            return Summary;
        }

        public double[] getRadialInPipe(double[] r)
        {
            double[] inner = new double[r.Length];
            double P1 = getPressureInCore();
            //inner[i] = (-((P3 * r3 * r3) / (r3 * r3 - r2 * r2)) - ((P3 * r3 * r3 * r2 * r2) / (r[i] * r[i] * (r3 * r3 - r2 * r2))));
            for (int i = 0; i < r.Length; i++)
                inner[i] = (-((P3 * r1 * r1) / (r1 * r1 - r2 * r2)) - ((P3 * r1 * r1 * r2 * r2) / (r[i] * r[i] * (r1 * r1 - r2 * r2))));
            //inner[i] = (P2 * r2 * r2 - P1 * r1 * r1) / (r1 * r1 - r2 * r2) + (P2 - P1) * r1 * r1 * r2 * r2 / (r[i] * r[i] * (r1 * r1 - r2 * r2));

            return inner;
        }

        public double[] getRadialInHomogen(double[] r) 
        {
            double[] homoTention = new double[r.Length];
            for (int i = 0; i < r.Length; i++)
            {
                homoTention[i] = ((P2 * r2 * r2 / (r3 * r3 - r2 * r2)) * (1 - (r3 * r3 / (r[i] * r[i]))));
                // (((p2 * r2 * r2) / (r1 * r1 - r2 * r2)) * (1 + ((r1 * r1) / (r[i] * r[i]))));
            }

            return homoTention;
        }

        public double[] getRadialSummary(double[] r, double[] Homo, double[] Inner, double[] Outer)
        {
            double[] Summary = new double[r.Length];
            for (int i = 0; i < r.Length; i++)
            {
                if (r[i] < r1)
                {
                    Summary[i] = (Homo[i] + Inner[i]);

                }
                else if (r[i] >= r1)
                {
                    Summary[i] = (Homo[i] + Outer[i]);
                }

            }
            return Summary;
        }
        #endregion


        #region === ReturnProps ===
        public double getR1() { return r1; }
        public double getR2() { return r2; }
        public double getR3() { return r3; }
        public double getP1() { return P2; }
        public double getAng() { return Ang; }
        public int getN() { return n; }
        public int getInd(double[] r)
        {
            for (int i = 0; i < r.Length; ++i)
            {
                if ((int)r[i] == (int)r1)
                    return i;
            }
            return r.Length / 2;
        }
        #endregion

    }
}
