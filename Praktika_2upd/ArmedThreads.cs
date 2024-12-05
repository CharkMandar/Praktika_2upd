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

        double r2, r1, r3, Ang, P1, P3, L;
        int n;

        const double ThreadThickness = 8.936;
        const double LD = 115;
        const double ro = 1.44;
        public ArmedThreads(double innerR, double outerR, double Angl, int amount, double innerP, double Step) 
        {
            n = amount;
            r2 = innerR;
            r3 = outerR;
            r1 = outerR+(ThreadThickness*n);
            Ang = Angl;
            P1 = innerP;
            L = 2*Math.PI*r3/Math.Tan(Ang);
        }

        //конуструктор по умолчанию
        public ArmedThreads()
        {
            r2 = 1;
            r1 = 2;
            r3 = 1.5;
            Ang = 0;
            n = 1;
            P1 = 1;
            L = 1;
        }

        #region === TentionFuncs ===
        public double[] getTentionInCore(double[] r)
        {
            double[] coreTention = new double[r.Length];
            for (int i = getInd(r); i < r.Length; i++)
            {
                coreTention[i] = (P1 * r[i] * L ) / (n * Math.Sin(Ang) * ThreadThickness * ThreadThickness * 100);
            }

            return coreTention;
        }

        public double[] getTentionInPipe(double[] r)
        {
            double[] inner = new double[r.Length];
            for(int i = 0; i <= getInd(r) ; i++)
                 inner[i] = (-((P3 * r3 * r3) / (r3 * r3 - r2 * r2)) - ((P3 * r3 * r3 * r2 * r2) / (r[i] * r[i] * (r3 * r3 - r2 * r2))));
            
            return inner;
        }


        public double[] getTentionInHomogen(double[] r )
        {
            double[] homoTention = new double[r.Length];
            for (int i = 0; i < r.Length; i++)
            {
                homoTention[i] = ((P1 * r2 * r2 / (r1 * r1 - r2 * r2)) * (1 + (r1 * r1 / (r[i] * r[i]))));
               // (((p2 * r2 * r2) / (r1 * r1 - r2 * r2)) * (1 + ((r1 * r1) / (r[i] * r[i]))));
            }

            return homoTention;
        }


        public double[] getSummaryTention(double[] r, double[] Homo, double[] Inner, double[] Outer)
        {
            double[] Summary = new double[r.Length];
            for (int i = 0; i < r.Length; i++)
            {
                if (i < getInd(r))
                {
                    Summary[i] = (Homo[i] + Inner[i]);

                }
                else if (i >= getInd(r))
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
        public double getP1() { return P1; }
        public double getAng() { return Ang; }
        public int getN() { return n; }
        public int getInd(double[] r)
        {
            for (int i = 0; i < r.Length; ++i)
            {
                if ((int)r[i] == (int)r3)
                    return i;
            }
            return r.Length / 2;
        }

        #endregion

    }
}
