using System.Linq;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;


namespace CobayeStudio
{
    public class PartitionTester
    {
        [Test]
        public void PartitionCorrectionWhenSumTooHigh()
        {
            float[] initialValues = new float[10]
            {
                0.5f, 0.003f, 0.075f, 0.09f, 0.12f, 0.0159f, 0.0137f, 0.035f, 0.237f, 0.0349f
            };

            PartitionManagement.CorrectPartition(initialValues);

            Assert.AreEqual(1.00000f, initialValues.Sum());
        }

        [Test]
        public void PartitionCorrectionWhenSumTooLow()
        {
            float[] initialValues = new float[10]
            {
                0.3f, 0.003f, 0.075f, 0.09f, 0.12f, 0.0159f, 0.0137f, 0.035f, 0.237f, 0.0349f
            };

            PartitionManagement.CorrectPartition(initialValues);

            Assert.AreEqual(1.00000f, initialValues.Sum());
        }

        [Test]
        public void NoPartitionCorrectionWhenSumIsOne()
        {
            float[] initialValues = new float[10]
            {
                0.3755f, 0.003f, 0.075f, 0.09f, 0.12f, 0.0159f, 0.0137f, 0.035f, 0.237f, 0.0349f
            };

            float[] storedValues = new float[10]
            {
                0.3755f, 0.003f, 0.075f, 0.09f, 0.12f, 0.0159f, 0.0137f, 0.035f, 0.237f, 0.0349f
            };

            PartitionManagement.CorrectPartition(initialValues);

            Assert.AreEqual(storedValues, initialValues);
        }
    }
}
