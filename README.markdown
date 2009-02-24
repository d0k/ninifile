NIniFile
========

IniFile is a portable INI file manipulation class loosely based on the VCL's
`TIniFile`/`TCustomIniFile` design. It is written in C# 2.0 and supported on
.NET Framework 2.0 and up and Mono 1.2 and up.

Basic Usage
-----------

Since IniFile implements the IDisposable interface it can be used with C#'s
`using` statement.

    using (IniFile ini = new IniFile("test.ini")) {
        // [section]
        // key=foo
        string x = ini.ReadString("section", "key", "default");
        // x is now "foo", or if the key was not found "default"

        ini.WriteDoubleInvariant("section", "key", 1.0);
        // the file will now look like this:
        // [section]
        // key=1.0

        // the difference between WriteDouble and WriteDoubleInvariant is
        // that the latter ignores system specific settings
        // (e. g. different decimal seperators)

        ini.DeleteKey("section", "key");
        // remove the key "key"
    }
    // changes are written to the disk when the object is disposed
    // if you want to flush the objects cache use UpdateFile()