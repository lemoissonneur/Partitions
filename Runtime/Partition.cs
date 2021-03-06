using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CobayeStudio
{
    /// <summary>
    /// Base class for PropertyDrawer
    /// </summary>
    [Serializable]
    public abstract class PartitionBase
    {
        protected abstract IList _elements { get; }

        /// <summary>
        /// Base class for PropertyDrawer
        /// </summary>
        [Serializable] public class ElementBase
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
            if (_elements.Count == 0) return -1;
            if (value <= 0.0f) return 0;
            if (value >= 1.0f) return _elements.Count - 1;

            int index = 0;
            float sum = (_elements[index] as ElementBase).Value;

            while (value > sum)
            {
                sum += (_elements[++index] as ElementBase).Value;
            }

            return index;
        }

        /// <summary>
        /// Shorthand for Elements[GetIndex(value)]
        /// </summary>
        /// <param name="value">value in the 0-1 range</param>
        /// <returns>null if no element found</returns>
        public ElementBase GetElement(float value)
        {
            int index = GetIndex(value);
            if (index != -1) return _elements[index] as ElementBase;
            else return null;
        }

        public void AddElement(ElementBase element, PartitionEditRule rule = PartitionEditRule.Default)
        {
            _elements.Add(element);

            CorrectPartition(_elements.Count - 1, rule);
        }

        public void RemoveElementAt(int index, PartitionEditRule rule = PartitionEditRule.Default)
        {
            if (index >= 0 && index < _elements.Count)
            {
                _elements.RemoveAt(index);

                CorrectPartition(_elements.Count - 1, rule);
            }
        }

        public void SetValues(float[] values)
        {
            if (values.Length == _elements.Count)
            {
                for (int i = 0; i < _elements.Count; i++)
                    (_elements[i] as ElementBase).Value = values[i];
            }

            CorrectPartition();
        }

        public void SetValue(int index, float value, PartitionEditRule rule = PartitionEditRule.Default)
        {
            (_elements[index] as ElementBase).Value = value;

            CorrectPartition(index, rule);
        }

        protected void CorrectPartition(int index = 0, PartitionEditRule rule = PartitionEditRule.Default)
        {
            // copy values
            float[] values = new float[_elements.Count];
            for (int i = 0; i < _elements.Count; i++) values[i] = (_elements[i] as ElementBase).Value;

            // correct values with the rule
            PartitionManagement.CorrectPartition(values, index, rule);

            // re applly corrected values
            for (int i = 0; i < _elements.Count; i++) (_elements[i] as ElementBase).Value = values[i];
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class Partition : PartitionBase
    {
        protected override IList _elements => Elements;

        /// <summary>
        /// List of Elements in the partition
        /// </summary>
        public List<Element> Elements = new List<Element>();

        [Serializable] public class Element : ElementBase { }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class Partition<T> : PartitionBase
    {
        protected override IList _elements => Elements;

        /// <summary>
        /// List of Elements in the partition
        /// </summary>
        public List<Element> Elements = new List<Element>();

        /// <summary>
        /// Serialized element of the partition
        /// </summary>
        [Serializable] public class Element : ElementBase
        {
            /// <summary>
            /// Corresponding data for the elements
            /// </summary>
            public T Object;
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

        public bool Contains(T obj)
        {
            foreach (Element e in Elements)
                if (obj.Equals(e.Object))
                    return true;
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum PartitionEditRule
    {
        AdjustAll = 0,          // edit all (if over, reduce all others) (if under raise all others)
        AdjustLeftAndRight = 1, // edit left and right
        AdjustLeftOnly = 2,     // edit left
        AdjustRightOnly = 3,    // edit right

        Default = 0
    }

    public static class PartitionManagement
    {
        /// <summary>
        /// Check a partition, and apply Default correction
        /// </summary>
        /// <param name="values">an array of values</param>
        public static void CorrectPartition(float[] values)
        {
            for (int i = 0; i < values.Length; i++)
                if (values[i] < 0.0f)
                    values[i] = 0.0f;

            float sum = values.Sum();
            float delta = 1.0f - sum;
            
            if(delta != 0.0f)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] += delta * values[i] / sum;
                }
            }
        }
        
        /// <summary>
        /// Correct a partition with specific rules
        /// </summary>
        /// <param name="values">array of values</param>
        /// <param name="index">last edited value</param>
        /// <param name="rule">rule for partition edit</param>
        public static void CorrectPartition(float[] values, int index, PartitionEditRule rule = PartitionEditRule.Default)
        {
            if (rule == PartitionEditRule.Default)
            {
                CorrectPartition(values);
            }
            else
            {
                for (int i = 0; i < values.Length; i++)
                    if (values[i] < 0.0f)
                        values[i] = 0.0f;

                float sum = values.Sum();
                float delta = 1.0f - sum;
                int range = 1;
                
                while (delta != 0.0f)
                {
                    switch (rule)
                    {
                        case PartitionEditRule.AdjustLeftAndRight:
                            if ((index - range) < 0) rule = PartitionEditRule.AdjustRightOnly;
                            else if ((index + range) > values.Length - 1) rule = PartitionEditRule.AdjustLeftOnly;
                            else
                            {
                                float halfDelta = delta / 2;
                                values[index + range] = Mathf.Max(0.0f, values[index + range] + halfDelta);
                                values[index - range] = Mathf.Max(0.0f, values[index - range] + halfDelta);
                            }
                            break;

                        case PartitionEditRule.AdjustLeftOnly:
                            if((index - range) < 0) rule = PartitionEditRule.Default;
                            else
                                values[index - range] = Mathf.Max(0.0f, values[index - range] + delta);
                            break;

                        case PartitionEditRule.AdjustRightOnly:
                            if ((index + range) >= values.Length - 1) rule = PartitionEditRule.Default;
                            else
                                values[index + range] = Mathf.Max(0.0f, values[index + range] + delta);
                            break;

                        case PartitionEditRule.Default:
                            CorrectPartition(values);
                            break;
                    }
                    delta = 1.0f - values.Sum();
                    range++;
                }
            }
        }
    }
}
