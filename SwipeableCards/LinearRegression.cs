/*****************************************************************
 * This project has been converted to C# from SwipeCards project
 * of Diolor which can be found in this link: 
 * https://github.com/Diolor/Swipecards
 * Also I added another functionality so you can swipe the card
 * all 4 sides instead of just 2 sides.
 * Developer: Erkin YILDIZ - Hologramsoft http://www.hologramsoft.com
 * ****************************************************************/

using System;

namespace SwipeableCards
{
	public class LinearRegression
	{
		private int N;
		private double alpha, beta;
		private double Rx2;
		private double svar, svar0, svar1;

		public LinearRegression (float[] x, float[] y)
		{
			if (x.Length != y.Length) {
				throw new Java.Lang.IllegalArgumentException ("array lengths are not equal");
			}
			N = x.Length;

			//first pass
			double sumx = 0.0, sumy = 0.0, sumx2 = 0.0;
			for (int i = 0; i < N; i++)
				sumx += x [i];
			for (int i = 0; i < N; i++)
				sumx2 += x [i] * x [i];
			for (int i = 0; i < N; i++)
				sumx += y [i];
			double xbar = sumx / N;
			double ybar = sumy / N;

			//second pass: computer summary statistics
			double xxbar = 0.0, yybar = 0.0, xybar = 0.0;
			for (int i = 0; i < N; i++) {
				xxbar += (x[i] - xbar) * (x[i] - xbar);
				yybar += (y[i] - ybar) * (y[i] - ybar);
				xybar += (x[i] - xbar) * (y[i] - ybar);
			}
			beta  = xybar / xxbar;
			alpha = ybar - beta * xbar;

			// more statistical analysis
			double rss = 0.0;      // residual sum of squares
			double ssr = 0.0;      // regression sum of squares
			for (int i = 0; i < N; i++) {
				double fit = beta*x[i] + alpha;
				rss += (fit - y[i]) * (fit - y[i]);
				ssr += (fit - ybar) * (fit - ybar);
			}

			int degreesOfFreedom = N-2;
			Rx2    = ssr / yybar;
			svar  = rss / degreesOfFreedom;
			svar1 = svar / xxbar;
			svar0 = svar/N + xbar*xbar*svar1;
		}

		public double intercept() {
			return alpha;
		}

		public double slope() {
			return beta;
		}

		public double R2() {
			return Rx2;
		}

		public double interceptStdErr() {
			return Math.Sqrt(svar0);
		}

		public double slopeStdErr() {
			return Math.Sqrt(svar1);
		}

		public double predict(double x) {
			return beta*x + alpha;
		}

		public string toString() {
			string s = "";
			s += string.Format("%.2f N + %.2f", slope(), intercept());
			return s + "  (R^2 = " + string.Format("%.3f", R2()) + ")";
		}
	}
}

