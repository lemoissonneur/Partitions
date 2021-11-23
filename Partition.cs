using System;
using System.Collections.Generic;
using UnityEngine;


namespace CobayeStudio
{
    /// <summary>
    /// Base class for PropertyDrawer
    /// </summary>
    [Serializable]
    public class PartitionBase
    {
        /// <summary>
        /// Base class for PropertyDrawer
        /// </summary>
        [Serializable] public class Element { }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class Partition : PartitionBase
    {
        /// <summary>
        /// List of Elements in the partition
        /// </summary>
        public List<Element> Elements = new List<Element>();

        /// <summary>
        /// Serialized element of the partition
        /// </summary>
        [Serializable]
        public new class Element : PartitionBase.Element
        {
            /// <summary>
            /// Color of the part in inspector
            /// </summary>
            public Color Color = Color.gray;

            /// <summary>
            /// Allocated amount
            /// </summary>
            public float Value;
        }

        /// <summary>
        /// Return the index of elements at the given value in the partition range
        /// </summary>
        /// <param name="value">value in the 0-1 range</param>
        /// <returns>Return 0 if given value is lower than 0, last index of elements list if value is more than 1, or -1 if list is empty</returns>
        public int GetIndex(float value)
        {
            if (Elements.Count == 0) return -1;
            if (value <= 0.0f) return 0;
            if (value >= 1.0f) return Elements.Count - 1;

            int index = 0;
            float sum = Elements[index].Value;

            while (value > sum)
            {
                sum += Elements[++index].Value;
            }

            return index;
        }

        /// <summary>
        /// Shorthand for Elements[GetIndex(value)]
        /// </summary>
        /// <param name="value">value in the 0-1 range</param>
        /// <returns>null if no element found</returns>
        public Element GetElement(float value)
        {
            int index = GetIndex(value);
            if (index != -1) return Elements[index];
            else return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class Partition<T> : PartitionBase
    {
        /// <summary>
        /// List of Elements in the partition
        /// </summary>
        public List<Element> Elements;

        /// <summary>
        /// Serialized element of the partition
        /// </summary>
        [Serializable]
        public new class Element : PartitionBase.Element
        {
            /// <summary>
            /// Color of the part in inspector
            /// </summary>
            public Color Color = Color.gray;

            /// <summary>
            /// Allocated amount
            /// </summary>
            public float Value;

            /// <summary>
            /// Corresponding data for the elements
            /// </summary>
            public T Object;
        }

        /// <summary>
        /// Return the index of elements at the given value in the partition range
        /// </summary>
        /// <param name="value">value in the 0-1 range</param>
        /// <returns>Return 0 if given value is lower than 0, last index of elements list if value is more than 1, or -1 if list is empty</returns>
        public int GetIndex(float value)
        {
            if (Elements.Count == 0) return -1;
            if (value <= 0.0f) return 0;
            if (value >= 1.0f) return Elements.Count - 1;

            int index = 0;
            float sum = Elements[index].Value;

            while (value > sum)
            {
                sum += Elements[++index].Value;
            }

            return index;
        }

        /// <summary>
        /// Shorthand for Elements[GetIndex(value)]
        /// </summary>
        /// <param name="value">value in the 0-1 range</param>
        /// <returns>null if no element found</returns>
        public Element GetElement(float value)
        {
            int index = GetIndex(value);
            if (index != -1) return Elements[index];
            else return null;
        }

        /// <summary>
        /// Shorthand for Elements[GetIndex(value)].Object
        /// </summary>
        /// <param name="value">value in the 0-1 range</param>
        /// <returns>default if no element found</returns>
        public T GetObject(float value)
        {
            int index = GetIndex(value);
            if (index != -1) return Elements[index].Object;
            else return default;
        }


    }
}
