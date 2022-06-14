//2022-05-25-1915
//
//  adapted from https://github.com/marcopacini/INI-Reader
//
//  GetValue(key)
//  SetValue(key, value)
//  Reload()
//  Ignores [SECTIONS] and ;comments
//  renames MyIniFile.ini to MyIniFile.ini.old
//

using System;
using System.Collections.Generic;
using System.IO;

namespace UTIL
{
    public class IniFile
    {
        private Dictionary<string,string> dictionary;
        private string iniFilePath;

        public IniFile(string filepath)
        {
            iniFilePath = filepath;
            dictionary = new Dictionary<string,string>();
            dictionary = LoadDictionary(filepath);
        }
        public bool Exists()
        {
            if (File.Exists(iniFilePath)) { return true; } else { return false; }
        }
        public string GetValue(string key)
        {
            if (dictionary.ContainsKey(key)) {
                return dictionary[key];
            } else
            {
                return String.Empty;
            }
        }
        public bool SetValue(string key, string value)
        {
            bool result = false;

            if ((key == String.Empty) || (key == null))
                return result;

            string inBuffer = String.Empty;

            if (File.Exists(iniFilePath))
            {
                inBuffer = File.ReadAllText(iniFilePath);

                //can't write ini.old file if under Program Files
                //File.Copy(iniFilePath, iniFilePath + ".old", true);
            }

            if (inBuffer.Contains(key))
            {
                string outBuffer = String.Empty;

                using (StringReader reader = new StringReader(inBuffer))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if ((!line.StartsWith("[")) && (!line.StartsWith(";")))
                        {
                            if (line.StartsWith(key))
                            {
                                line = key.Trim() + "=" + value.Trim();
                            }
                        }
                        outBuffer += line + Environment.NewLine;
                    }
                }
                File.WriteAllText(iniFilePath, outBuffer);
                result = true;
            }
            else
            {
                File.AppendAllText(iniFilePath, key + "=" + value + 
                    Environment.NewLine, System.Text.Encoding.Unicode);
            }
            return result;
        }
        public void Reload()
        {
            dictionary.Clear();
            dictionary = LoadDictionary(iniFilePath);
        }
        //public string GetAllText()
        //{
        //    string inBuffer = File.ReadAllText(iniFilePath);
        //    return inBuffer;
        //}
        //public Dictionary<string,string> GetDictionary()
        //{
        //    return dictionary;
        //}
        private Dictionary<string,string> LoadDictionary(string filepath)
        {
            var dict = new Dictionary<string,string>();

            if (File.Exists(filepath))
            {
                foreach (var line in File.ReadAllLines(filepath))
                {
                    string key = string.Empty;
                    string value = string.Empty;

                    string row = line.Trim();

                    if ((!row.StartsWith("[")) && (!row.StartsWith(";")))
                    {
                        int pos = row.IndexOf('=');

                        if (pos != -1)
                        {
                            key = row.Substring(0, pos);
                            value = row.Substring(pos + 1);
                        }
                        if (key != string.Empty)
                            dict.Add(key.Trim(), value.Trim());
                    }
                }
            }
            return dict;
        }           
    }
}
