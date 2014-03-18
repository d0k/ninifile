/* IniFile.cs -- INI reader
 *
 * Copyright (c) 2009 Benjamin Kramer
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 *
 */

namespace NIniFile
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;

    /// <summary>
    /// This class implments a parser for INI-style files.
    /// It is loosely based on the VCL's TCustomIniFile.
    /// All changes are cached locally until written out with UpdateFile()
    /// </summary>
    public class IniFile : IDisposable
    {
        private readonly List<string> lines_ = new List<string>();

        protected string this[int index] {
            get { return lines_[index]; }
            set { lines_[index] = value; }
        }

        /// <summary>
        /// Gets the number of lines.
        /// </summary>
        protected int Count {
            get { return lines_.Count; }
        }

        private readonly string fileName_;
        /// <summary>
        /// Gets the name of the associated file.
        /// </summary>
        public virtual string FileName {
            get { return fileName_; }
        }

        /// <summary>
        /// Adds a line to the bottom of the file.
        /// </summary>
        protected void Add(string line) {
            lines_.Add(line);
        }

        /// <summary>
        /// Inserts a line at the specified index.
        /// </summary>
        protected void Insert(int index, string line) {
            lines_.Insert(index, line);
        }

        /// <summary>
        /// Removes te line at the specified index.
        /// </summary>
        protected void RemoveLine(int index) {
            lines_.RemoveAt(index);
        }

        /// <summary>
        /// Initializes a new IniFile and caches the
        /// contents of fileName if it exists.
        /// </summary>
        public IniFile(string fileName) {
            fileName_ = fileName;
            if (File.Exists(fileName)) {
                using (StreamReader reader = new StreamReader(fileName)) {
                    while (reader.Peek() != -1)
                        Add(reader.ReadLine().Trim());
                }
            }
        }

        /// <summary>
        /// Dispose the object and save the file.
        /// </summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing)
                UpdateFile();
        }

        /// <summary>
        /// Write cached contents to file. The file is created if it
        /// doesn't exist.
        /// </summary>
        public virtual void UpdateFile() {
            using (StreamWriter writer = new StreamWriter(FileName)) {
                for (int i = 0; i < Count; i++)
                    writer.WriteLine(this[i]);
            }
        }

        /// <summary>
        /// Removes comments (starting with ;) and supoerflous
        /// whitespace from line
        /// </summary>
        protected static string StripComments(string line) {
            if (line != null) {
                if (line.IndexOf(';') != -1)
                    return line.Remove(line.IndexOf(';')).Trim();
                return line.Trim();
            }
            return string.Empty;
        }

        private int SkipToSection(string name) {
            if (name != null) {
                string needle = "[" + name + "]";
                for (int i = 0; i < Count; i++) {
                    if (StripComments(this[i]) == needle)
                        return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Checks if the specified section exists.
        /// </summary>
        public virtual bool SectionExists(string name) {
            return SkipToSection(name) != -1;
        }

        /// <summary>
        /// Deletes Key <b>name</b> in section <b>section</b>.
        /// </summary>
        public virtual void DeleteKey(string section, string name) {
            int i = SkipToSection(section);
            if (i != -1) {
                for (; i < Count; i++) {
                    string line = this[i];
                    if (line.StartsWith(name + '=', StringComparison.Ordinal)
                        || line.StartsWith(name + " =",
                                           StringComparison.Ordinal)) {
                        RemoveLine(i);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Completely removes the specified section
        /// </summary>
        public virtual void EraseSection(string section) {
            int i = SkipToSection(section);
            if (i != -1) {
                RemoveLine(i);

                for (; i < Count; i++) {
                    string line = StripComments(this[i]);
                    if (line.Length != 0 && line[0] == '['
                        && line[line.Length - 1] == ']')
                        return;

                    RemoveLine(i);
                }
            }
        }

        /// <summary>
        /// Gets the contents at the specified key as a string.
        /// If the key doesn't exist an empty string is returned.
        /// </summary>
        public virtual string ReadString(string section, string key) {
            return ReadString(section, key, String.Empty);
        }

        private int FindKey(string key, int i) {
            if (key != null) {
                for (; i < Count; i++) {
                    string line = StripComments(this[i]);
                    if (line.StartsWith(key + '=', StringComparison.Ordinal)
                        || line.StartsWith(key + " =", StringComparison.Ordinal))
                        return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Gets the contents at the specified key as a string.
        /// </summary>
        /// <param name="defaultvalue">
        /// Is returned if the key doesn't exist
        /// </param>
        public virtual string ReadString(string section, string key,
                                         string defaultvalue) {
            int i = SkipToSection(section);
            if (i != -1) {
                i = FindKey(key, i);
                if (i != -1) {
                    string line = StripComments(this[i]);
                    char[] trimmer = new char[] { ' ', '"', '\r' };
                    return line.Substring(line.IndexOf('=') + 1).Trim(trimmer);
                }
            }
            return defaultvalue;
        }

        /// <summary>
        /// Writes the string <b>value</b> to the key <b>key</b> in the section
        /// <b>section</b>. The key/section will be created if the key and/or
        /// section doesn't exist.
        /// </summary>
        public virtual void WriteString(string section, string key,
                                        string value) {
            if (section == null || key == null || value == null)
                return;
            string newLine = key + '=' + value;
            int i = SkipToSection(section);
            if (i == -1) {
                Add("[" + section + "]");
                Add(newLine);
            } else {
                i++;
                int j = FindKey(key, i);
                if (j != -1)
                    this[j] = newLine;
                else
                    Insert(i + 1, newLine);
            }
        }

        /// <summary>
        /// Gets the contents at the specified key as a bool.
        /// </summary>
        /// <param name="defaultvalue">
        /// Is returned if the key doesn't exist or couldn't be parsed
        /// </param>
        public virtual bool ReadBool(string section, string key,
                                     bool defaultvalue) {
            bool ret;
            if (bool.TryParse(ReadString(section, key), out ret))
                return ret;
            return defaultvalue;
        }

        /// <summary>
        /// Writes <b>value</b> to the key <b>key</b> in the section
        /// <b>section</b>.
        /// </summary>
        public virtual void WriteBool(string section, string key, bool value) {
            WriteString(section, key, value.ToString());
        }

        /// <summary>
        /// Gets the contents at the specified key as a integer.
        /// </summary>
        /// <param name="defaultvalue">
        /// Is returned if the key doesn't exist or couldn't be parsed
        /// </param>
        public virtual int ReadInteger(string section, string key,
                                       int defaultvalue) {
            int ret;
            if (int.TryParse(ReadString(section, key), NumberStyles.Integer,
                            CultureInfo.InvariantCulture, out ret))
                return ret;
            return defaultvalue;
        }

        /// <summary>
        /// Writes <b>value</b> to the key <b>key</b> in the section
        /// <b>section</b>.
        /// </summary>
        public virtual void WriteInteger(string section, string key,
                                         int value) {
            WriteString(section, key,
                        value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Gets the contents at the specified key as a double.
        /// </summary>
        /// <param name="defaultvalue">
        /// Is returned if the key doesn't exist or couldn't be parsed
        /// </param>
        /// <param name="provider">A FormatProvider to read the data.</param>
        public virtual double ReadDouble(string section, string key,
                                         double defaultvalue,
                                         IFormatProvider provider) {
            double ret;
            if (double.TryParse(ReadString(section, key), NumberStyles.Float,
                                provider, out ret))
                return ret;
            return defaultvalue;
        }

        /// <summary>
        /// Gets the contents at the specified key as a double.
        /// The invariant culture is used to format the string.
        /// (i.e. decimal separator = '.')
        /// </summary>
        /// <param name="defaultvalue">
        /// Is returned if the key doesn't exist or couldn't be parsed
        /// </param>
        public virtual double ReadDoubleInvariant(string section, string key,
                                                  double defaultvalue) {
            return ReadDouble(section, key, defaultvalue,
                              CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets the contents at the specified key as a double.
        /// The current culture is used to format the string.
        /// (e.g. decimal separator = ',' on german OS)
        /// </summary>
        /// <param name="defaultvalue">
        /// Is returned if the key doesn't exist or couldn't be parsed
        /// </param>
        public virtual double ReadDouble(string section, string key,
                                         double defaultvalue) {
            return ReadDouble(section, key, defaultvalue,
                              CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Writes <b>value</b> to the key <b>key</b> in the section
        /// <b>section</b>.
        /// </summary>
        /// <param name="provider">A FormatProvider to format the data.</param>
        public virtual void WriteDouble(string section, string key,
                                        double value,
                                        IFormatProvider provider) {
            WriteString(section, key, value.ToString(provider));
        }

        /// <summary>
        /// Writes <b>value</b> to the key <b>key</b> in the section
        /// <b>section</b>. The invariant culture is used to format the string.
        /// (i.e. decimal separator = '.')
        /// </summary>
        public virtual void WriteDoubleInvariant(string section, string key,
                                                 double value) {
            WriteDouble(section, key, value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Writes <b>value</b> to the key <b>key</b> in the section
        /// <b>section</b>. The current culture is used to format the string.
        /// (e.g. decimal separator = ',' on german OS)
        /// </summary>
        public virtual void WriteDouble(string section, string key,
                                        double value) {
            WriteDouble(section, key, value, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Gets the contents at the specified key as a DateTime.
        /// </summary>
        /// <param name="defaultvalue">
        /// Is returned if the key doesn't exist or couldn't be parsed
        /// </param>
        /// <param name="provider">A FormatProvider to format the data.</param>
        public DateTime ReadDateTime(string section, string key,
                                     DateTime defaultvalue,
                                     IFormatProvider provider) {
            DateTime ret;
            if (DateTime.TryParse(ReadString(section, key), provider,
                                  DateTimeStyles.None, out ret))
                return ret;
            return defaultvalue;
        }

        /// <summary>
        /// Gets the contents at the specified key as a DateTime.
        /// </summary>
        /// <param name="format">A format string to format the data</param>
        /// <param name="defaultvalue">
        /// Is returned if the key doesn't exist or couldn't be parsed
        /// </param>
        /// <param name="provider">A FormatProvider to format the data.</param>
        public DateTime ReadDateTime(string section, string key, string format,
                                     DateTime defaultvalue,
                                     IFormatProvider provider) {
            DateTime ret;
            if (DateTime.TryParseExact(ReadString(section, key), format,
                                       provider, DateTimeStyles.None, out ret))
                return ret;
            return defaultvalue;
        }

        /// <summary>
        /// Writes <b>value</b> to the key <b>key</b> in the section
        /// <b>section</b>.
        /// </summary>
        /// <param name="provider">A FormatProvider to format the data.</param>
        public void WriteDateTime(string section, string key, DateTime value,
                                  IFormatProvider provider) {
            WriteString(section, key, value.ToString(provider));
        }

        /// <summary>
        /// Writes <b>value</b> to the key <b>key</b> in the section
        /// <b>section</b>.
        /// </summary>
        /// <param name="format">
        /// A format string to format the data
        /// </param>
        /// <param name="provider">
        /// A FormatProvider to format the data.
        /// </param>
        public void WriteDateTime(string section, string key, string format,
                                  DateTime value, IFormatProvider provider) {
            WriteString(section, key, value.ToString(format, provider));
        }
    }
}
