using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;

namespace WEbScraperForRuggedTablets
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = @"";
            string[] lines = System.IO.File.ReadAllLines(url);

            List<Dictionary<string, string>> tablets = new List<Dictionary<string, string>>();

            int LineCount = 1;
            foreach (string line in lines)
            {
                string html = GetHTMLFromUrl(lines[8]);

                var dict = ExtractDetailsToDictionaryFromHtml(html);

                tablets.Add(dict);

                Console.WriteLine($"Line {LineCount++} new tablet added");

                if (LineCount >= 1) break;
            }

            // collects all the existing keys from all dicts
            SortedSet<string> attributes = new SortedSet<string>();
            foreach (var dict in tablets)
            {
                foreach (string key in dict.Keys)
                {
                    attributes.Add(key);
                    Console.WriteLine($"added new key string: {key}");
                }
            }

            Console.WriteLine("Collecting attributes finished.");

            // fill up dicts with the missing keys with empty values
            foreach (var dict in tablets)
            {
                foreach (string key in attributes)
                {
                    Console.WriteLine($"checking keys: {key}");
                    if (!dict.ContainsKey(key))
                    {
                        Console.WriteLine("added empty string");
                        dict.Add(key, "");
                    }
                }
            }

            Console.WriteLine("Completing dictionaries with blanks finished.\n");

            foreach (string key in attributes)
            {
                Console.WriteLine($"{key}: {tablets[0][key]}");
            }

            Console.ReadKey();
        }

        private static Dictionary<string, string> ExtractDetailsToDictionaryFromHtml(string html)
        {
            html = html.Replace("\n", "");
            
            string[] tags = html.Split('<');

            var dictionary = new Dictionary<string, string>();

            bool NameFound = false;
            bool ReviewLinkFound = false;
            bool LinksRefFound = false;
            string key = "";

            for (int i = 0; i < tags.Length; i++)
            {
                // find name
                if (!NameFound &&
                    tags[i].Split('>').Length > 1 &&
                    tags[i].Split('>')[1].Trim() != "")
                {
                    string name = tags[i].Split('>')[1].Trim();
                    dictionary.Add("Name", name);
                    NameFound = true;
                    // Console.WriteLine(name);
                }
                // find review link
                else if (!ReviewLinkFound &&
                    tags[i].Length > 2 &&
                    tags[i].Substring(0, 2).ToLower() == "a ")
                {
                    dictionary.Add("See review", tags[i].Split('"')[1]);
                    ReviewLinkFound = true;
                }
                // find next key
                else if (key == "" &&
                    tags[i].Split('>').Length > 1 &&
                    tags[i].Split('>')[1].Trim() != "")
                {
                    key = tags[i].Split('>')[1].Trim();
                }
                // find next value
                else if (key != "")
                {
                    if (tags[i].Length > 2 &&
                        tags[i].Substring(0, 2).ToLower() == "a ")
                    {
                        string value = tags[i].Split('"')[1];
                        LinksRefFound = true;
                        if (dictionary.ContainsKey(key))
                        {
                            dictionary[key] = $"{dictionary[key]}, {value}";
                        }
                        else
                        {
                            dictionary.Add(key, value);
                        }
                        key = "";
                    }
                    else if (tags[i].Split('>').Length > 1 &&
                        tags[i].Split('>')[1].Trim() != "")
                    {
                        if (LinksRefFound)
                        {
                            LinksRefFound = false;
                        }
                        else
                        {
                            string value = tags[i].Split('>')[1].Trim();
                            if (dictionary.ContainsKey(key))
                            {
                                dictionary[key] = $"{dictionary[key]}, {value}";
                            }
                            else
                            {
                                dictionary.Add(key, value);
                            }
                            key = "";
                        }
                    }
                }
                    
            }

            return dictionary;
        }

        private static string GetHTMLFromUrl(string url)
        {
            string html = "";

            using (WebClient wc = new WebClient())
            {
                html = wc.DownloadString(url);
            }

            return html;
        }
    }
}
