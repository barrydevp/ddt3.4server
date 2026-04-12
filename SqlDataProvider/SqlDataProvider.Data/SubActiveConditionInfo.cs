using System.Collections;
using System.Collections.Generic;

namespace SqlDataProvider.Data
{
    public class SubActiveConditionInfo
    {
        public int ActiveID
        {
            get;
            set;
        }

        public int AwardType
        {
            get;
            set;
        }

        public string AwardValue
        {
            get;
            set;
        }

        public int ConditionID
        {
            get;
            set;
        }

        public int ID
        {
            get;
            set;
        }

        public bool IsValid
        {
            get;
            set;
        }

        public int SubID
        {
            get;
            set;
        }

        public int Type
        {
            get;
            set;
        }

        private string m_value;
        private Dictionary<string, string> m_valueDict;
        public string Value
        {
            get
            {
                return m_value;
            }
            set
            {
                m_value = value;
                m_valueDict = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(Value))
                {
                    string[] array = Value.Split('-');
                    for (int i = 1; i < array.Length; i += 2)
                    {
                        string key = array[i - 1];
                        if (!m_valueDict.ContainsKey(key))
                        {
                            m_valueDict.Add(key, array[i]);
                        }
                        else
                        {
                            m_valueDict[key] = array[i];
                        }
                    }
                }
            }
        }

        public int GetValue(string index)
        {
            //Dictionary<string, string> dictionary = new Dictionary<string, string>();
            //int result;
            //if (!string.IsNullOrEmpty(Value))
            //{
            //    string[] array = Value.Split('-');
            //    for (int i = 1; i < array.Length; i += 2)
            //    {
            //        string key = array[i - 1];
            //        if (!dictionary.ContainsKey(key))
            //        {
            //            dictionary.Add(key, array[i]);
            //        }
            //        else
            //        {
            //            dictionary[key] = array[i];
            //        }
            //    }
            //    if (dictionary.ContainsKey(index))
            //    {
            //        result = int.Parse(dictionary[index]);
            //        return result;
            //    }
            //}
            //result = 0;
            //return result;
            if (m_valueDict.ContainsKey(index))
            {
                return int.Parse(m_valueDict[index]);
            }

            return 0;
        }
    }
}
