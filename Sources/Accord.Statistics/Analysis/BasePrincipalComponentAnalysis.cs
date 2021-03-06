// Accord Statistics Library
// The Accord.NET Framework
// http://accord-framework.net
//
// Copyright © César Souza, 2009-2015
// cesarsouza at gmail.com
//
//    This library is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Lesser General Public
//    License as published by the Free Software Foundation; either
//    version 2.1 of the License, or (at your option) any later version.
//
//    This library is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Lesser General Public
//    License along with this library; if not, write to the Free Software
//    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

namespace Accord.Statistics.Analysis.Base
{
    using System;
    using System.Collections.ObjectModel;
    using Accord.Math;
    using Accord.Math.Comparers;
    using Accord.Math.Decompositions;
    using Accord.MachineLearning;

    /// <summary>
    ///   Base class for Principal Component Analyses.
    /// </summary>
    /// 
    [Serializable]
    public abstract class BasePrincipalComponentAnalysis
    {

        private int numberOfInputs;
        private int numberOfComponents;

        private double[] columnMeans;
        private double[] columnStdDev;

        private double[][] eigenvectors;
        private double[] eigenvalues;
        private double[] singularValues;
        private double[] componentProportions;
        private double[] componentCumulative;

        private bool overwriteSourceMatrix;
        private PrincipalComponentMethod analysisMethod;
        private bool whiten;

        private PrincipalComponentCollection componentCollection;


        // TODO: Remove
        /// <summary>Obsolete</summary>
        protected double[,] source;
        /// <summary>Obsolete</summary>
        protected double[][] array;
        /// <summary>Obsolete</summary>
        protected double[,] result;
        /// <summary>Obsolete</summary>
        protected bool onlyCovarianceMatrixAvailable;
        /// <summary>Obsolete</summary>
        protected double[,] covarianceMatrix;
        /// <summary>Obsolete</summary>
        protected bool saveResult = false;



        /// <summary>
        /// Initializes a new instance of the <see cref="BasePrincipalComponentAnalysis"/> class.
        /// </summary>
        /// 
        public BasePrincipalComponentAnalysis()
        {
            columnMeans = new double[0];
            columnStdDev = new double[0];
            eigenvectors = Jagged.Zeros(0, 0);
            eigenvalues = new double[0];
            singularValues = new double[0];
            componentCumulative = new double[0];
            componentProportions = new double[0];
        }

        /// <summary>
        ///   Gets the column standard deviations of the source data given at method construction.
        /// </summary>
        /// 
        public double[] StandardDeviations
        {
            get { return this.columnStdDev; }
            set { this.columnStdDev = value; }
        }

        /// <summary>
        ///   Gets the column mean of the source data given at method construction.
        /// </summary>
        /// 
        public double[] Means
        {
            get { return this.columnMeans; }
            set { this.columnMeans = value; }
        }

        /// <summary>
        ///   Gets or sets the method used by this analysis.
        /// </summary>
        /// 
        public PrincipalComponentMethod Method
        {
            get { return this.analysisMethod; }
            set { this.analysisMethod = value; }
        }

        /// <summary>
        ///   Gets or sets whether calculations will be performed overwriting
        ///   data in the original source matrix, using less memory.
        /// </summary>
        /// 
        public bool Overwrite
        {
            get { return overwriteSourceMatrix; }
            set { overwriteSourceMatrix = value; }
        }

        /// <summary>
        ///   Gets or sets whether the transformation result should be whitened
        ///   (have unit standard deviation) before it is returned.
        /// </summary>
        public bool Whiten
        {
            get { return whiten; }
            set { whiten = value; }
        }

        /// <summary>
        ///   Gets the number of inputs (dimensionality of the input vectors)
        ///   expected by this analysis.
        /// </summary>
        /// 
        public int NumberOfInputs
        {
            get { return numberOfInputs; }
            protected set { numberOfInputs = value; }
        }

        /// <summary>
        ///   Gets or sets the number of outputs (dimensionality of the output vectors)
        ///   that should be generated by this model.
        /// </summary>
        /// 
        public int NumberOfOutputs
        {
            get { return numberOfComponents; }
            set
            {
                if (MaximumNumberOfOutputs > 0 && value > MaximumNumberOfOutputs)
                    throw new ArgumentOutOfRangeException("value", "Number of outputs should be less than or equal the number of columns in the input data {0}.".Format(MaximumNumberOfOutputs));
                numberOfComponents = value;
            }
        }

        /// <summary>
        ///   Gets the maximum number of outputs (dimensionality of the output vectors)
        ///   that can be generated by this model.
        /// </summary>
        /// 
        public int MaximumNumberOfOutputs
        {
            get { return componentProportions.Length; }
        }

        /// <summary>
        ///   Gets or sets the amount of explained variance that should be generated
        ///   by this model. This value will alter the <see cref="NumberOfOutputs"/>
        ///   that can be generated by this model.
        /// </summary>
        /// 
        public double ExplainedVariance
        {
            get { return componentProportions[NumberOfOutputs]; }
            set { NumberOfOutputs = GetNumberOfComponents(value); }
        }


        /// <summary>
        ///   Provides access to the Singular Values stored during the analysis.
        ///   If a covariance method is chosen, then it will contain an empty vector.
        /// </summary>
        /// 
        /// <value>The singular values.</value>
        /// 
        public double[] SingularValues
        {
            get { return singularValues; }
            protected set { singularValues = value; }
        }

        /// <summary>
        ///   Returns the original data supplied to the analysis.
        /// </summary>
        /// 
        /// <value>The original data matrix supplied to the analysis.</value>
        /// 
        [Obsolete("This property will be removed.")]
        public double[,] Source
        {
            get
            {
                if (this.source == null)
                    this.source = array.ToMatrix();
                return this.source;
            }
            private set
            {
                if (value != source)
                    source = value;
            }
        }

        /// <summary>
        ///   Gets the resulting projection of the source
        ///   data given on the creation of the analysis 
        ///   into the space spawned by principal components.
        /// </summary>
        /// 
        /// <value>The resulting projection in principal component space.</value>
        /// 
        [Obsolete("This property will be removed.")]
        public double[,] Result
        {
            get { return this.result; }
            protected set { this.result = value; }
        }

        /// <summary>
        ///   Gets a matrix whose columns contain the principal components. Also known as the Eigenvectors or loadings matrix.
        /// </summary>
        /// 
        /// <value>The matrix of principal components.</value>
        /// 
        [Obsolete("Please use ComponentValues instead.")]
        public double[,] ComponentMatrix
        {
            get { return this.eigenvectors.Transpose().ToMatrix(); }
        }

        /// <summary>
        ///   Gets a matrix whose columns contain the principal components. Also known as the Eigenvectors or loadings matrix.
        /// </summary>
        /// 
        /// <value>The matrix of principal components.</value>
        /// 
        public double[][] ComponentVectors
        {
            get { return this.eigenvectors; }
            protected set { this.eigenvectors = value; }
        }

        /// <summary>
        ///   Provides access to the Eigenvalues stored during the analysis.
        /// </summary>
        /// 
        /// <value>The Eigenvalues.</value>
        /// 
        public double[] Eigenvalues
        {
            get { return eigenvalues; }
            protected set { eigenvalues = value; }
        }

        /// <summary>
        ///   The respective role each component plays in the data set.
        /// </summary>
        /// 
        /// <value>The component proportions.</value>
        /// 
        public double[] ComponentProportions
        {
            get { return componentProportions; }
        }

        /// <summary>
        ///   The cumulative distribution of the components proportion role. Also known
        ///   as the cumulative energy of the principal components.
        /// </summary>
        /// 
        /// <value>The cumulative proportions.</value>
        /// 
        public double[] CumulativeProportions
        {
            get { return componentCumulative; }
        }


        /// <summary>
        ///   Gets the Principal Components in a object-oriented structure.
        /// </summary>
        /// 
        /// <value>The collection of principal components.</value>
        /// 
        public PrincipalComponentCollection Components
        {
            get { return componentCollection; }
        }

        /// <summary>
        ///   Returns the minimal number of principal components
        ///   required to represent a given percentile of the data.
        /// </summary>
        /// 
        /// <param name="threshold">The percentile of the data requiring representation.</param>
        /// <returns>The minimal number of components required.</returns>
        /// 
        public int GetNumberOfComponents(double threshold)
        {
            if (threshold < 0 || threshold > 1.0)
                throw new ArgumentException("Threshold should be a value between 0 and 1", "threshold");

            for (int i = 0; i < componentCumulative.Length; i++)
            {
                if (componentCumulative[i] >= threshold)
                    return i + 1;
            }

            return componentCumulative.Length;
        }


        /// <summary>
        ///   Creates additional information about principal components.
        /// </summary>
        /// 
        protected void CreateComponents()
        {
            int numComponents = singularValues.Length;
            componentProportions = new double[numComponents];
            componentCumulative = new double[numComponents];

            // Calculate proportions
            double sum = 0.0;
            for (int i = 0; i < eigenvalues.Length; i++)
                sum += System.Math.Abs(eigenvalues[i]);
            sum = (sum == 0) ? 0.0 : (1.0 / sum);

            for (int i = 0; i < componentProportions.Length; i++)
                componentProportions[i] = System.Math.Abs(eigenvalues[i]) * sum;

            // Calculate cumulative proportions
            this.componentCumulative[0] = this.componentProportions[0];
            for (int i = 1; i < this.componentCumulative.Length; i++)
                this.componentCumulative[i] = this.componentCumulative[i - 1] + this.componentProportions[i];

            // Creates the object-oriented structure to hold the principal components
            var components = new PrincipalComponent[singularValues.Length];
            for (int i = 0; i < components.Length; i++)
                components[i] = new PrincipalComponent(this, i);
            this.componentCollection = new PrincipalComponentCollection(components);

            if (NumberOfOutputs == 0 || NumberOfOutputs > MaximumNumberOfOutputs)
                NumberOfOutputs = MaximumNumberOfOutputs;
        }
    }
}
