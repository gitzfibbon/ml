/*

BIAS-VARIANCE DECOMPOSITION FOR ZERO-ONE LOSS

Version 1.0
http://homes.cs.washington.edu/~pedrod/bvd.c

Pedro Domingos
Department of Computer Science and Engineering
University of Washington
Box 352350
Seattle, WA 98195-2350
pedrod@cs.washington.edu

April 18, 2000


The C functions in this file implement the bias-variance decomposition for
zero-one loss described in the paper "A Unified Bias-Variance Decomposition."
Zero noise is assumed. biasvar() computes the average zero-one loss, bias, net
variance, and components of the variance on a test set of examples. biasvarx()
computes the loss, bias and variance on an individual example.


biasvar() has the following parameters:

classes:  a vector containing the actual classes of the test examples,
          represented as integers; classes[i] is the class of test example i

preds:    an array where preds[i][j] is the class predicted for example i
          by the classifier learned on training set j; classes are represented
          as integers, consistent with those used in the classes vector

ntestexs: the number of test examples used

ntrsets:  the number of training sets used

loss:     pointer to a float where the average loss is returned

bias:     pointer to a float where the average bias is returned

var:      pointer to a float where the net variance is returned

varp:     pointer to a float where the average contribution to variance
          from unbiased examples is returned

varn:     pointer to a float where the average contribution to variance
          from biased examples is returned

varc:     pointer to a float where the average contribution to variance from
          biased examples is returned, with the variance from each example
          weighted by the probability that the class predicted for the example
          is the optimal prediction, given that it is not the main
          prediction. (In multiclass domains, the net variance is equal to
          varp - varc, not varp - varn.)


biasvarx() has the following parameters:

classx:   the actual class of the example, represented as an integer

predsx:   a vector where predsx[j] is the class predicted for the
          example by the classifier learned on training set j; classes are
          represented as integers, consistent with classx

ntrsets:  the number of training sets used

lossx:    pointer to a float where the loss on the example is returned

biasx:    pointer to a float where the bias on the example is returned

varx:     pointer to a float where the variance on the example is returned


Copyright (C) 2000 Pedro Domingos

This code is free software; you can redistribute it and/or modify it under
the terms of the GNU Lesser General Public License as published by the Free
Software Foundation. See http://www.gnu.org/copyleft/lesser.html.

This code is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR
A PARTICULAR PURPOSE.  See the GNU Lesser General Public License for more
details.

*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bagging
{
    public class BiasVariance
    {
        private const int MaxTestExs = 10000;
        private const int MaxTrSets = 100;
        private const int MaxClasses = 100;

        public static void biasvar(List<int> classes, List<List<int>> preds, int ntestexs, int ntrsets)
        {
            double loss = 0;
            double bias = 0;
            double var = 0;
            double varp = 0;
            double varn = 0;
            double varc = 0;

            BiasVariance.biasvar(classes, preds, ntestexs, ntrsets, ref loss, ref bias, ref var, ref varp, ref varn, ref varc);
        }

        public static void biasvar(List<int> classes, List<List<int>> preds, int ntestexs, int ntrsets, ref double loss, ref double bias, ref double var, ref double varp, ref double varn, ref double varc)
        {
            double lossx = 0;
            double biasx = 0;
            double varx = 0;

            loss = 0.0;
            bias = 0.0;
            varp = 0.0;
            varn = 0.0;
            varc = 0.0;

            for (int e = 0; e < ntestexs; e++)
            {
                BiasVariance.biasvarx(classes[e], preds[e], ntrsets, ref lossx, ref biasx, ref varx);
                loss += lossx;
                bias += biasx;
                if (biasx != 0.0)
                {
                    varn += varx;
                    varc += 1.0;
                    varc -= lossx;
                }
                else
                    varp += varx;
            }

            loss /= ntestexs;
            bias /= ntestexs;
            var = loss - bias;
            varp /= ntestexs;
            varn /= ntestexs;
            varc /= ntestexs;

            Trace.TraceInformation("");
            Trace.TraceInformation("loss,bias,var,varp,varn,varc");
            Trace.TraceInformation("{0},{1},{2},{3},{4},{5}", loss, bias, var, varp, varn, varc);
        }

        private static void biasvarx(int classx, List<int> predsx, int ntrsets, ref double lossx, ref double biasx, ref double varx)
        {
            int[] nclass = new int[BiasVariance.MaxClasses];
            int majclass = -1;
            int nmax = 0;

            for (int c = 0; c < BiasVariance.MaxClasses; c++)
            {
                nclass[c] = 0;
            }

            for (int t = 0; t < ntrsets; t++)
            {
                nclass[predsx[t]]++;
            }

            for (int c = 0; c < BiasVariance.MaxClasses; c++)
            {
                if (nclass[c] > nmax)
                {
                    majclass = c;
                    nmax = nclass[c];
                }
            }

            lossx = 1.0 - (double)nclass[classx] / ntrsets;
            biasx = Convert.ToDouble(majclass != classx);
            varx = 1.0 - (double)nclass[majclass] / ntrsets;
        }


    }
}
