using System;
using System.Collections.Generic;
using UnityEngine;


namespace CobayeStudio
{
    [Serializable]
    public class PartitionBase
    {
        [Serializable] public class Element { }
    }

    [Serializable]
    public class Partition : PartitionBase
    {
        public List<Element> Elements = new List<Element>();

        [Serializable]
        public new class Element : PartitionBase.Element
        {
            public Color Color = Color.gray;
            public float Value;
        }

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

        public Element GetElement(float value)
        {
            int index = GetIndex(value);
            if (index != -1) return Elements[index];
            else return null;
        }
    }

    [Serializable]
    public class Partition<T> : PartitionBase
    {
        public List<Element> Elements;

        [Serializable]
        public new class Element : PartitionBase.Element
        {
            public Color Color = Color.gray;
            public float Value;
            public T Object;
        }

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

        public Element GetElement(float value)
        {
            int index = GetIndex(value);
            if (index != -1) return Elements[index];
            else return null;
        }

        public T GetObject(float value)
        {
            int index = GetIndex(value);
            if (index != -1) return Elements[index].Object;
            else return default;
        }


    }
}
