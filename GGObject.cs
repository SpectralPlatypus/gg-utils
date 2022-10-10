using System.Text.Json;
using System.Collections;

namespace GGUtils
{
    enum GGValue
    {
        Null = 1,  // don't read anything
        Dictionary = 2,
        Array = 3,
        String = 4,
        Int = 5,
        Float = 6,
        Point = 9,  // read from string array (x,y) or {x,y} 
        Rect = 10,  // x,y,w,h  read from string array {{%g,%g},{%g,%g}} or ()
        Quad = 11,  // 4 set of floats as string {{%g,%g},{%g,%g},{%g,%g},{%g,%g}} or ()
    }
    abstract class GGObject
    {
        virtual public void Serialize(Utf8JsonWriter writer) 
        {
            writer.WriteNullValue();
        }

        public T? To<T>() where T : GGObject
        {
            if (this is T)
                return (T)this;

            return null;
        }

        public bool Is<T>() where T : GGObject
        {
            return (this is T);
        }
    }

    internal class GGNull : GGObject { }

    internal class GGDictionary : GGObject, IEnumerable<KeyValuePair<string, GGObject>>
    {
        Dictionary<string, GGObject> dictionary;

        public GGDictionary()
        {
            dictionary = new Dictionary<string, GGObject>();
        }

        public GGDictionary(Dictionary<string, GGObject> dict)
        {
            this.dictionary = dict;
        }

        public GGObject this[string i]
        {
            get { return dictionary[i]; }
            set { dictionary[i] = value; }
        }

        public void Add(string key, GGObject value) => dictionary.Add(key, value);

        public void Remove(string key) => dictionary.Remove(key);

        public GGObject Get(string key) => dictionary[key];

        override public void Serialize(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            foreach ((string key, GGObject value) in dictionary)
            {
                writer.WritePropertyName(key);
                value.Serialize(writer); 
            }
            writer.WriteEndObject();
        }



        public IEnumerator<KeyValuePair<string, GGObject>> GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }
    }

    internal class GGArray : GGObject, IEnumerable<GGObject>
    {
        List<GGObject> data;

        public int Length => data.Count;

        public GGArray()
        {
            data = new List<GGObject>();
        }

        public GGArray(List<GGObject> list)
        {
            this.data = list;
        }

        public GGObject this[int i]
        {
            get { return data[i]; }
            set { data[i] = value; }
        }

        public void Add(GGObject obj) => data.Add(obj);

        public void Remove(GGObject obj) => data.Remove(obj);

        override public void Serialize(Utf8JsonWriter writer)
        {
            writer.WriteStartArray();
            foreach (GGObject obj in data)
            {
                obj.Serialize(writer);
            }
            writer.WriteEndArray();
        }

        public IEnumerator<GGObject> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return data.GetEnumerator();
        }
    }

    internal class GGString : GGObject
    {
        public string Value { get; set; }

        public GGString(string value)
        {
            Value = value;
        }

        public GGString() : this("")
        {
        }

        override public void Serialize(Utf8JsonWriter writer)
        {
            writer.WriteStringValue(Value);
        }

        public override string ToString() => Value;
    }

    // Technically float and int only 
    internal class GGPod<T> : GGObject where T : struct
    {
        public T Value { get; set; }
       
        public GGPod(T value)
        {
            Value = value;
        }

        override public void Serialize(Utf8JsonWriter writer)
        {
            if (Value is float f)
                writer.WriteNumberValue(f);
            else if (Value is int i)
                writer.WriteNumberValue(i);
            else
                writer.WriteStringValue(Value.ToString());
        }

        public override string ToString() => Value.ToString() ?? "ERR";
    }
}
