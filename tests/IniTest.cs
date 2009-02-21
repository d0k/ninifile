namespace NIniFile.Test
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
            using (StreamWriter writer = new StreamWriter(fileName)) {
                writer.WriteLine("[Section1] ; dumb comment");
                writer.WriteLine("foo=bar");
                writer.WriteLine("[Test]");
                writer.WriteLine("; testest");
                writer.WriteLine(";[NotASection]");
                writer.WriteLine("d=1.23");
                writer.WriteLine("garbage");
                writer.WriteLine("d2=1,23");
                writer.WriteLine("b=false");
                writer.WriteLine("i=23; foo");
                writer.WriteLine("date=2009-02-13 23:31:30");
                writer.WriteLine("file = \"acme payroll.dat\"");
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
                Assert.AreEqual("acme payroll.dat", ini.ReadString("Test", "file"));
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
        public void EraseSectionTop() {
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
        public void EraseSectionBottom() {
            using (IniFile ini = new IniFile(fileName)) {
                Assert.IsTrue(ini.SectionExists("Test"));
                ini.EraseSection("Test");
                Assert.IsFalse(ini.SectionExists("Test"));
                Assert.IsTrue(ini.SectionExists("Section1"));
            }
            using (IniFile ini = new IniFile(fileName)) {
                Assert.IsFalse(ini.SectionExists("Test"));
                Assert.IsTrue(ini.SectionExists("Section1"));
            }
        }

        [Test]
        public void DeleteKey() {
            using (IniFile ini = new IniFile(fileName)) {
                Assert.AreNotEqual(String.Empty, ini.ReadString("Test", "b", String.Empty));
                Assert.AreNotEqual(String.Empty, ini.ReadString("Test", "file", String.Empty));
                ini.DeleteKey("Test", "b");
                ini.DeleteKey("Test", "file");
                Assert.AreEqual(String.Empty, ini.ReadString("Test", "b", String.Empty));
                Assert.AreEqual(String.Empty, ini.ReadString("Test", "file", String.Empty));
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
        }

        [Test]
        public void WriteDoubleInvariant() {
            using (IniFile ini = new IniFile(fileName)) {
                Assert.AreNotEqual(23.42, ini.ReadDoubleInvariant("Section1", "doesnotexist", 0));
                ini.WriteDoubleInvariant("Section1", "doesnotexist", 23.42);
                Assert.AreEqual(23.42, ini.ReadDoubleInvariant("Section1", "doesnotexist", 0));
            }
        }

        [Test]
        public void ReadDateTime() {
            DateTime date = new DateTime(2009, 2, 13, 23, 31, 30);
            using (IniFile ini = new IniFile(fileName)) {
                Assert.AreEqual(date, ini.ReadDateTime("Test", "date", new DateTime(), CultureInfo.InvariantCulture));
                Assert.AreEqual(date, ini.ReadDateTime("Test", "date", "yyyy-MM-dd HH:mm:ss", new DateTime(), CultureInfo.InvariantCulture));
            }
        }

        [Test]
        public void WriteDateTime() {
            DateTime date = new DateTime(2009, 2, 13, 23, 31, 30);
            using (IniFile ini = new IniFile(fileName)) {
                Assert.AreNotEqual(date, ini.ReadDateTime("Section1", "doesnotexist", new DateTime(), CultureInfo.InvariantCulture));
                ini.WriteDateTime("Section1", "doesnotexist", date, CultureInfo.InvariantCulture);
                Assert.AreEqual(date, ini.ReadDateTime("Section1", "doesnotexist", new DateTime(), CultureInfo.InvariantCulture));
            }
        }

        [Test]
        public void WriteDateTimeFormatted() {
            DateTime date = new DateTime(2009, 2, 13, 23, 31, 30);
            DateTime date2 = new DateTime(2009, 1, 1, 0, 0, 0);
            using (IniFile ini = new IniFile(fileName)) {
                Assert.AreNotEqual(date2, ini.ReadDateTime("Section1", "doesnotexist", "yyyy" , new DateTime(), CultureInfo.InvariantCulture));
                ini.WriteDateTime("Section1", "doesnotexist", "yyyy", date, CultureInfo.InvariantCulture);
                Assert.AreEqual(date2, ini.ReadDateTime("Section1", "doesnotexist", "yyyy", new DateTime(), CultureInfo.InvariantCulture));
            }
        }

        [TearDown]
        public void Cleanup() {
            File.Delete(fileName);
        }
    }
}
