/**
 * Copyright by Nocksoft
 * https://www.nocksoft.de
 * https://nocksoft.de/tutorials/visual-c-sharp-arbeiten-mit-ini-dateien/
 * https://github.com/Nocksoft/INIFile.cs
 * -----------------------------------
 * Author:  Rafael Nockmann @ Nocksoft
 * Updated: 2022-01-09
 * Version: 1.0.3
 *
 * Language: Visual C#
 *
 * License: MIT license
 * License URL: https://github.com/Nocksoft/INIFile.cs/blob/master/LICENSE
 * 
 * Description:
 * Provides basic functions for working with INI files.
 *
 * ##############################################################################################
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Nocksoft.IO.ConfigFiles
{
    public class INIFile
    {
        private string _File;

        /// <summary>
        /// Call the constructor creates a new object of the INIFile class to work with INI files.
        /// </summary>
        /// <param name="file">Name of INI file, which you want to access.</param>
        /// <param name="createFile">Specifies whether the INI file should be created if it does not exist.</param>
        public INIFile(string file, bool createFile = false)
        {
            if (createFile == true && File.Exists(file) == false)
            {
                FileInfo fileInfo = new FileInfo(file);
                FileStream fileStream = fileInfo.Create();
                fileStream.Close();
            }
            _File = file;
        }

        #region Public Methods

        /// <summary>
        /// Removes all comments and empty lines from a complete section and returns the sections.
        /// This method is not case-sensitive.
        /// The return value does not contain any spaces at the beginning or at the end of a line.
        /// </summary>
        /// <param name="section">Name of the requested section.</param>
        /// <param name="includeComments">Specifies whether comments should also be returned.</param>
        /// <returns>Returns the whole section.</returns>
        public List<string> GetSection(string section, bool includeComments = false)
        {
            section = CheckSection(section);

            List<string> completeSection = new List<string>();
            bool sectionStart = false;

            string[] fileArray = File.ReadAllLines(_File);

            foreach (var item in fileArray)
            {
                if (item.Length <= 0) continue;

                // Beginning of section.
                if (item.Replace(" ", "").ToLower() == section)
                {
                    sectionStart = true;
                }
                // Beginning of next section.
                if (sectionStart == true && item.Replace(" ", "").ToLower() != section && item.Replace(" ", "").Substring(0, 1) == "[" && item.Replace(" ", "").Substring(item.Length - 1, 1) == "]")
                {
                    break;
                }
                if (sectionStart == true)
                {
                    // Add the entry to the List<string> completeSection, if it is not a comment or an empty entry.
                    if (includeComments == false
                        && item.Replace(" ", "").Substring(0, 1) != ";" && !string.IsNullOrWhiteSpace(item))
                    {
                        completeSection.Add(ReplaceSpacesAtStartAndEnd(item));
                    }
                    if (includeComments == true && !string.IsNullOrWhiteSpace(item))
                    {
                        completeSection.Add(ReplaceSpacesAtStartAndEnd(item));
                    }
                }
            }
            return completeSection;
        }

        /// <summary>
        /// The method returns a value for the associated key.
        /// This method is not case-sensitive.
        /// </summary>
        /// <param name="section">Name of the requested section.</param>
        /// <param name="key">Name of the requested key.</param>
        /// <param name="convertValueToLower">If "true" is passed, the value will be returned in lowercase letters.</param>
        /// <returns>Returns the value for the specified key in the specified section, if available, otherwise null.</returns>
        public string GetValue(string section, string key, bool convertValueToLower = false)
        {
            section = CheckSection(section);
            key = key.ToLower();

            List<string> completeSection = GetSection(section);

            foreach (var item in completeSection)
            {
                // Continue if entry is no key.
                if (!item.Contains("=") && item.Contains("[") && item.Contains("]")) continue;

                string[] keyAndValue = item.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                if (keyAndValue[0].ToLower() == key && keyAndValue.Count() > 1)
                {
                    if (convertValueToLower == true)
                    {
                        keyAndValue[1] = keyAndValue[1].ToLower();
                    }
                    return keyAndValue[1];
                }
            }
            return null;
        }

        /// <summary>
        /// Set or add a value of the associated key in the specified section.
        /// This method is not case-sensitive.
        /// </summary>
        /// <param name="section">Name of the section.</param>
        /// <param name="key">Name of the key.</param>
        /// <param name="value">Value to save.</param>
        /// <param name="convertValueToLower">If "true" is passed, the value will be saved in lowercase letters.</param>
        public void SetValue(string section, string key, string value, bool convertValueToLower = false)
        {
            section = CheckSection(section, false);
            string sectionToLower = section.ToLower();

            bool sectionFound = false;

            List<string> iniFileContent = new List<string>();

            string[] fileLines = File.ReadAllLines(_File);

            // Creates a new INI file if none exists.
            if (fileLines.Length <= 0)
            {
                iniFileContent = AddSection(iniFileContent, section, key, value, convertValueToLower);
                WriteFile(iniFileContent);
                return;
            }

            for (int i = 0; i < fileLines.Length; i++)
            {
                // Possibility 1: The desired section has not (yet) been found.
                if (fileLines[i].Replace(" ", "").ToLower() != sectionToLower)
                {
                    iniFileContent.Add(fileLines[i]);
                    // If a section does not exist, the section will be created.
                    if (i == fileLines.Length - 1 && fileLines[i].Replace(" ", "").ToLower() != sectionToLower && sectionFound == false)
                    {
                        iniFileContent.Add(null);
                        iniFileContent = AddSection(iniFileContent, section, key, value, convertValueToLower);
                        break;
                    }
                    continue;
                }


                // Possibility 2 -> Desired section was found.
                sectionFound = true;

                // Get the complete section in which the target key may be.
                List<string> targetSection = GetSection(sectionToLower, true);

                for (int x = 0; x < targetSection.Count; x++)
                {
                    string[] targetKey = targetSection[x].Split(new string[] { "=" }, StringSplitOptions.None);
                    // When the target key is found.
                    if (targetKey[0].ToLower() == key.ToLower())
                    {
                        if (convertValueToLower == true)
                        {
                            iniFileContent.Add(key + "=" + value.ToLower());
                        }
                        else
                        {
                            iniFileContent.Add(key + "=" + value);
                        }
                        i = i + x;
                        break;
                    }
                    else
                    {
                        iniFileContent.Add(targetSection[x]);
                        // If the target key is not found, it will be created.
                        if (x == targetSection.Count - 1 && targetKey[0].ToLower() != key.ToLower())
                        {
                            if (convertValueToLower == true)
                            {
                                iniFileContent.Add(key + "=" + value.ToLower());
                            }
                            else
                            {
                                iniFileContent.Add(key + "=" + value);
                            }
                            i = i + x;
                            break;
                        }
                    }
                }
            }

            WriteFile(iniFileContent);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Ensures that a section is always in the following format: [section].
        /// </summary>
        /// <param name="section">Section to be checked for correct format.</param>
        /// <param name="convertToLower">Specifies whether the section should be vonverted in lower case letters.</param>
        /// <returns>Returns section in this form: [section].</returns>
        private string CheckSection(string section, bool convertToLower = true)
        {
            if (convertToLower == true)
            {
                section = section.ToLower();
            }
            if (!section.StartsWith("[") && !section.EndsWith("]"))
            {
                section = "[" + section + "]";
            }
            return section;
        }

        /// <summary>
        /// Removes leading and trailing spaces from sections, keys and values.
        /// </summary>
        /// <param name="item">String to be trimmed.</param>
        /// <returns>Returns the trimmed string.</returns>
        private string ReplaceSpacesAtStartAndEnd(string item)
        {
            // If the string has a key and a value.
            if (item.Contains("=") && !item.Contains("[") && !item.Contains("]"))
            {
                string[] keyAndValue = item.Split(new string[] { "=" }, StringSplitOptions.None);
                return keyAndValue[0].Trim() + "=" + keyAndValue[1].Trim();
            }

            return item.Trim();
        }

        /// <summary>
        /// Adds a new section with key value pair.
        /// </summary>
        /// <param name="iniFileContent">List iniFileContent from SetValue.</param>
        /// <param name="section">Section to be created.</param>
        /// <param name="key">Key to be added.</param>
        /// <param name="value">Value to be added.</param>
        /// <param name="convertValueToLower">Specifies whether the key and value should be saved in lower case letters.</param>
        /// <returns>Returns the new created section with key value pair.</returns>
        private List<string> AddSection(List<string> iniFileContent, string section, string key, string value, bool convertValueToLower)
        {
            if (convertValueToLower == true)
            {
                value = value.ToLower();
            }

            iniFileContent.Add(section);
            iniFileContent.Add($"{key}={value}");
            return iniFileContent;
        }

        private void WriteFile(List<string> content)
        {
            StreamWriter writer = new StreamWriter(_File);
            foreach (var item in content)
            {
                writer.WriteLine(item);
            }
            writer.Close();
        }

        #endregion
    }
}
