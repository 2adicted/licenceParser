using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LicenceParser
{
    public class Smoother
    {
        public static int range = 5;
        public static double decay = 0.8;
        private double[] noisy;
        private double[] clean;

       

        public Smoother()
        {
           
        }

        public void set_noisy(int[] noisy_data)
        {
            noisy = new double[noisy_data.Length];
            for (int i = 0; i < noisy.Length - 1; i++) noisy[i] = System.Convert.ToDouble(noisy_data[i]);
            clean = new double[noisy.Length];
        }

        public double[] get_clean()
        {
            return CleanData(this.noisy, 5, 0.8);
        }


        private static double[] CleanData(double[] noisy, int range, double decay)
        {
            double[] clean = new double[noisy.Length];
            double[] coefficients = Coefficients(range, decay);

            double divisor = 0;
            for (int i = -range; i <= range; i++)
            {
                divisor += coefficients[Math.Abs(i)];
            }

            for (int i = range; i < clean.Length - range; i++)
            {
                double temp = 0;
                for (int j = -range; j<= range; j++)
                {
                    temp += noisy[i + j] * coefficients[Math.Abs(j)];
                }
                clean[i] = temp / divisor;
            }

            double leadSum = 0;
            double trailSum = 0;
            int leadRef = range;
            int trailRef = clean.Length - range - 1;
            for (int i = 1; i <= range; i++)
            {
                leadSum += (clean[leadRef] - clean[leadRef + i]) / i;
                trailSum += (clean[trailRef] - clean[trailRef - i]) / i;
            }
            double leadSlope = leadSum / range;
            double trailSlope = trailRef / range;

            for (int i = 1; i <= range; i++)
            {
                clean[leadRef - i] = clean[leadRef] + leadSlope * i;
                clean[trailRef + i] = clean[trailRef] + trailSlope * i;
            }
            return clean;
        }

        static private double[] Coefficients(int range, double decay)
        {
            double[] coefficients = new double[range + 1];
            for (int i = 0; i <= range; i++)
            {
                coefficients[i] = Math.Pow(decay, i);
            }

            return coefficients;
        }
    }
}
