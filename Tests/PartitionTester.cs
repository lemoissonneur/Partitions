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

        [Test]
        public void TestAddElement()
        {
            PartitionBase partition = new Partition();
            Partition.Element newElementA = new Partition.Element() { Color = Color.red };
            Partition.Element newElementB = new Partition.Element() { Color = Color.blue };

            partition.AddElement(newElementA);
            partition.AddElement(newElementB);

            partition.SetValues(new float[2] { 0.2f, 0.8f });
            
            Assert.AreEqual(Color.red, (partition as Partition).Elements[0].Color);
            Assert.AreEqual(0.2f, (partition as Partition).Elements[0].Value);

            Assert.AreEqual(Color.blue, (partition as Partition).Elements[1].Color);
            Assert.AreEqual(0.8f, (partition as Partition).Elements[1].Value);
        }

        [Test]
        public void TestAddElementGeneric()
        {
            PartitionBase partition = new Partition<Vector3>();
            Partition<Vector3>.Element newElementA = new Partition<Vector3>.Element() { Color = Color.red, Object = Vector3.up };
            Partition<Vector3>.Element newElementB = new Partition<Vector3>.Element() { Color = Color.blue, Object = Vector3.down };

            partition.AddElement(newElementA);
            partition.AddElement(newElementB);

            partition.SetValues(new float[2] { 0.2f, 0.8f });

            Assert.AreEqual(Color.red, (partition as Partition<Vector3>).Elements[0].Color);
            Assert.AreEqual(0.2f, (partition as Partition<Vector3>).Elements[0].Value);
            Assert.AreEqual(Vector3.up, (partition as Partition<Vector3>).Elements[0].Object);

            Assert.AreEqual(Color.blue, (partition as Partition<Vector3>).Elements[1].Color);
            Assert.AreEqual(0.8f, (partition as Partition<Vector3>).Elements[1].Value);
            Assert.AreEqual(Vector3.down, (partition as Partition<Vector3>).Elements[1].Object);
        }
    }
}
