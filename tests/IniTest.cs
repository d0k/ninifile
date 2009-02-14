namespace OldFormatsSharp
{
    using System;
    using System.Globalization;
    using System.IO;
    using NUnit.Framework;

    [TestFixture]
    public class IniTest
    {
        private const string fileName = "test.ini";

        [SetUp]
        public void Init() {
            using (StreamWriter writer = new StreamWriter("test.ini")) {
                writer.WriteLine("[Section1] ; dumb comment");
                writer.WriteLine("foo=bar");
                writer.WriteLine("[Test]");
                writer.WriteLine("; testest");
                writer.WriteLine(";[NotASection]");
                writer.WriteLine("d=1.23");
                writer.WriteLine("d2=1,23");
                writer.WriteLine("b=false");
                writer.WriteLine("i=23; foo");
            }
        }

        [Test]
        public void NewFile() {
            const string fileName = "NewFile.ini";
            using (IniFile ini = new IniFile(fileName)) {
                ini.WriteString("foo", "bar", "qux");
                Assert.AreEqual("qux", ini.ReadString("foo", "bar"));
            }
            Assert.IsTrue(File.Exists(fileName));
            File.Delete(fileName);
        }

        [Test]
        public void ReadString() {
            using (IniFile ini = new IniFile(fileName)) {
                Assert.AreEqual("bar", ini.ReadString("Section1", "foo"));
                Assert.AreEqual("23", ini.ReadString("Test", "i"));
            }
        }

        [Test]
        public void SectionExists() {
            using (IniFile ini = new IniFile(fileName)) {
                Assert.IsTrue(ini.SectionExists("Test"));
                Assert.IsFalse(ini.SectionExists("doesnotexisT"));
                Assert.IsTrue(ini.SectionExists("Section1"));
            }
        }

        [Test]
        public void WriteAndRead() {
            using (IniFile ini = new IniFile(fileName)) {
                ini.WriteString("foo", "bar", "qux");
                Assert.AreEqual("qux", ini.ReadString("foo", "bar"));
            }
            using (IniFile ini = new IniFile(fileName)) {
                ini.WriteString("foo", "bar", "qux");
                Assert.AreEqual("qux", ini.ReadString("foo", "bar"));
            }
            using (IniFile ini = new IniFile(fileName)) {
                Assert.AreEqual("qux", ini.ReadString("foo", "bar"));
            }
        }

        [Test]
        public void EraseSection() {
            using (IniFile ini = new IniFile(fileName)) {
                Assert.IsTrue(ini.SectionExists("Section1"));
                ini.EraseSection("Section1");
                Assert.IsFalse(ini.SectionExists("Section1"));
                Assert.IsTrue(ini.SectionExists("Test"));
            }
            using (IniFile ini = new IniFile(fileName)) {
                Assert.IsFalse(ini.SectionExists("Section1"));
                Assert.IsTrue(ini.SectionExists("Test"));
            }
        }

        [Test]
        public void DeleteKey() {
            using (IniFile ini = new IniFile(fileName)) {
                Assert.AreNotEqual(String.Empty, ini.ReadString("Test", "b", String.Empty));
                ini.DeleteKey("Test", "b");
                Assert.AreEqual(String.Empty, ini.ReadString("Test", "b", String.Empty));
            }
            using (IniFile ini = new IniFile(fileName)) {
                Assert.AreEqual(String.Empty, ini.ReadString("Test", "b", String.Empty));
            }
        }

        [Test]
        public void WriteStringNoSection() {
            using (IniFile ini = new IniFile(fileName)) {
                Assert.IsFalse(ini.SectionExists("doesnotexist"));
                ini.WriteString("doesnotexist", "foo", "bar");
                Assert.IsTrue(ini.SectionExists("doesnotexist"));
                Assert.AreEqual("bar", ini.ReadString("doesnotexist", "foo"));
            }
            using (IniFile ini = new IniFile(fileName)) {
                Assert.IsTrue(ini.SectionExists("doesnotexist"));
                Assert.AreEqual("bar", ini.ReadString("doesnotexist", "foo"));
            }
        }

        [Test]
        public void WriteStringKeyExists() {
            using (IniFile ini = new IniFile(fileName)) {
                Assert.AreEqual("bar", ini.ReadString("Section1", "foo"));
                ini.WriteString("Section1", "foo", "qux");
                Assert.AreEqual("qux", ini.ReadString("Section1", "foo"));
            }
            using (IniFile ini = new IniFile(fileName)) {
                Assert.AreEqual("qux", ini.ReadString("Section1", "foo"));
            }
        }

        [Test]
        public void WriteStringKeyDoesNotExist() {
            using (IniFile ini = new IniFile(fileName)) {
                Assert.AreEqual(String.Empty, ini.ReadString("Section1", "bar"));
                ini.WriteString("Section1", "bar", "foo");
                Assert.AreEqual("foo", ini.ReadString("Section1", "bar"));
            }
            using (IniFile ini = new IniFile(fileName)) {
                Assert.AreEqual("foo", ini.ReadString("Section1", "bar"));
            }
        }

        [Test]
        public void ReadBool() {
            using (IniFile ini = new IniFile(fileName)) {
                Assert.IsFalse(ini.ReadBool("Test", "b", true));
                Assert.IsFalse(ini.ReadBool("Test", "i", false));
                Assert.IsTrue(ini.ReadBool("Test", "doesnotexist", true));
            }
        }

        [Test]
        public void WriteBool() {
            using (IniFile ini = new IniFile(fileName)) {
                Assert.IsFalse(ini.ReadBool("Section1", "doesnotexist", false));
                ini.WriteBool("Section1", "doesnotexist", true);
                Assert.IsTrue(ini.ReadBool("Section1", "doesnotexist", false));
            }
            using (IniFile ini = new IniFile(fileName)) {
                Assert.IsTrue(ini.ReadBool("Section1", "doesnotexist", false));
            }
        }

        [Test]
        public void ReadInteger() {
            using (IniFile ini = new IniFile(fileName)) {
                Assert.AreEqual(-99, ini.ReadInteger("Test", "doesnotexist", -99));
                Assert.AreEqual(23, ini.ReadInteger("Test", "i", 0));
            }
        }

        [Test]
        public void WriteInteger() {
            using (IniFile ini = new IniFile(fileName)) {
                Assert.AreNotEqual(42, ini.ReadInteger("Section1", "doesnotexist", 0));
                ini.WriteInteger("Section1", "doesnotexist", 42);
                Assert.AreEqual(42, ini.ReadInteger("Section1", "doesnotexist", 0));
            }
            using (IniFile ini = new IniFile(fileName)) {
                Assert.AreEqual(42, ini.ReadInteger("Section1", "doesnotexist", 0));
            }
        }

        [Test]
        public void ReadDouble() {
            using (IniFile ini = new IniFile(fileName)) {
                Assert.AreEqual(1.23, ini.ReadDoubleInvariant("Test", "d", 0));
                Assert.AreEqual(1.23, ini.ReadDouble("Test", "d2", 0, CultureInfo.GetCultureInfo("de-DE")));
            }
        }

        [Test]
        public void WriteDouble() {
            using (IniFile ini = new IniFile(fileName)) {
                Assert.AreNotEqual(23.42, ini.ReadDouble("Section1", "doesnotexist", 0));
                ini.WriteDouble("Section1", "doesnotexist", 23.42);
                Assert.AreEqual(23.42, ini.ReadDouble("Section1", "doesnotexist", 0));
            }
            using (IniFile ini = new IniFile(fileName)) {
                Assert.AreEqual(23.42, ini.ReadDouble("Section1", "doesnotexist", 0));
            }
        }

        [TearDown]
        public void Cleanup() {
            File.Delete("test.ini");
        }
    }
}