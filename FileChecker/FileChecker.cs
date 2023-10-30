using System;
using System.Collections.Generic;
using System.Linq;
using Vishnu.Interchange;
using System.IO;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace FileChecker
{
    /// <summary>
    /// Prüft, ob bestimmte Dateien in einem Pfad existieren
    /// und ggf. wie alt sie sind. Listet diese Dateien und ihre
    /// File-Infos in einem SubResultListContainer in ReturnObject.
    /// </summary>
    /// <remarks>
    /// File: FileChecker.cs
    /// Autor: Erik Nagel
    ///
    /// 05.04.2014 Erik Nagel: erstellt
    /// </remarks>
    public class FileChecker : INodeChecker
    {
        /// <summary>
        /// Kann aufgerufen werden, wenn sich der Verarbeitungsfortschritt
        /// des Checkers geändert hat, muss aber zumindest aber einmal zum
        /// Schluss der Verarbeitung aufgerufen werden.
        /// </summary>
        public event ProgressChangedEventHandler? NodeProgressChanged;

        /// <summary>
        /// Rückgabe-Objekt des Checkers
        /// </summary>
        public object? ReturnObject
        {
            get { return this._returnObject; }
            set { this._returnObject = value; }
        }

        /// <summary>
        /// Hier wird der (normalerweise externe) Arbeitsprozess ausgeführt (oder beobachtet).
        /// </summary>
        /// <param name="checkerParameters">Modus: SIZE=Dateigröße, COUNT=Datei-Anzahl, AGE=Zeit seit letzter Änderung, TRACE=Zeit der Überwachung<br></br>
        /// Dateipfad als regulärer Ausdruck|&lt; oder &gt;|kritische Anzahl oder kritisches Alter<br></br>
        /// Format von Alter: Einheit + ':' + ganzzahliger Wert; Einheit: S=Sekunden, M=Minuten, H=Stunden, D=Tage.</param>
        /// <param name="treeParameters">Für den gesamten Tree gültige Parameter oder null.</param>
        /// <param name="source">Auslösendes TreeEvent oder null.</param>
        /// <returns>True, False oder null</returns>
        public bool? Run(object? checkerParameters, TreeParameters treeParameters, TreeEvent source)
        {
            if (this._fatalException != null)
            {
                throw this._fatalException;
            }
            this.OnNodeProgressChanged(0);
            if (this._paraString != checkerParameters?.ToString())
            {
                this._tracedFiles.Clear();
            }
            this._paraString = checkerParameters?.ToString();
            this.evaluateParameters(this._paraString, treeParameters, source);

            this.OnNodeProgressChanged(50);
            bool? rtn = this.checkFiles();
            this._fileCheckerReturnObject.LogicalResult = rtn;
            this._returnObject = this._fileCheckerReturnObject;
            this.OnNodeProgressChanged(100);
            return rtn;
        }

        /// <summary>
        /// Standard Konstruktor.
        /// </summary>
        public FileChecker()
        {
            this._fatalException = null;
            this._paraString = null;
            this._returnObject = null;
            this._tracedFiles = new Dictionary<string, TracedFile>();
            this._fileCheckerReturnObject = new FileCheckerReturnObject();
        }

        private string? _paraString;
        private object? _returnObject = null;
        private FileCheckerReturnObject _fileCheckerReturnObject;
        private Dictionary<string, TracedFile> _tracedFiles;
        private Exception? _fatalException;
        private bool _exceptionToFalse;

        private class TracedFile
        {
            public DateTime Inserted;
            public FileInfo? Info;
        }

        private bool? checkFiles()
        {
            if (String.IsNullOrEmpty(this._fileCheckerReturnObject.FileMask) || String.IsNullOrEmpty(this._fileCheckerReturnObject.SearchDir))
            {
                return null;
            }
            this.ReturnObject = null;
            this._exceptionToFalse = true;
            bool? rtn = true;
            Regex reg;
            List<string> files;
            try
            {
                reg = new Regex(this._fileCheckerReturnObject.FileMask, RegexOptions.IgnoreCase);
                files = Directory.GetFiles(this._fileCheckerReturnObject.SearchDir)
                                 .Where(path => reg.IsMatch(path))
                                 .ToList();
                if (files.Count < 1)
                {
                    string trimmedFileMask = this._fileCheckerReturnObject.FileMask.TrimStart('\\');
                    string escapedFileMask = Regex.Escape(trimmedFileMask);
                    reg = new Regex(escapedFileMask, RegexOptions.IgnoreCase);
                    files = Directory.GetFiles(this._fileCheckerReturnObject.SearchDir)
                                     .Where(path => reg.IsMatch(path))
                                     .ToList();
                }
            }
            catch (ArgumentException ex)
            {
                FileNotFoundException exp = new FileNotFoundException(String.Format("Der Pfad {0} + {1} ist nicht gültig.",
                    this._fileCheckerReturnObject.SearchDir, this._fileCheckerReturnObject.FileMask), ex);
                if (!this._exceptionToFalse)
                {
                    throw exp;
                }
                files = new List<string>();
                rtn = false;
            }
            this._fileCheckerReturnObject.CountFiles = files.Count;
            if (this._fileCheckerReturnObject.Mode == "TRACE")
            {
                string[] tracedFileNames = this._tracedFiles.Keys.ToArray();
                foreach (string fileName in tracedFileNames)
                {
                    if (!files.Contains(fileName))
                    {
                        this._tracedFiles.Remove(fileName);
                    }
                }
            }
            this._fileCheckerReturnObject.SubResults?.SubResults?.Clear();
            if (!this.filesToResult(files))
            {
                rtn = false;
            }
            return rtn;
        }

        /// <summary>
        /// Wertet die übergebenen Parameter aus und speichert sie in _fileCheckerReturnObject.
        /// </summary>
        /// <param name="checkerParameters">Dateipfad mit Wildcards|&lt; oder &gt;|maximal erlaubte(s) Anzahl oder Alter<br></br>
        /// Format von Alter: Einheit + ':' + ganzzahliger Wert; Einheit: S=Sekunden, M=Minuten, H=Stunden, D=Tage."</param>
        /// <param name="treeParameters">Für den gesamten Tree gültige Parameter oder null.</param>
        /// <param name="source">Auslösendes TreeEvent oder null.</param>
        private void evaluateParameters(string? checkerParameters, object treeParameters, TreeEvent source)
        {
            string[] para = checkerParameters?.Split('|') ?? throw new ArgumentException("checkerParameters sind null.");
            string mode = para.Length > 0 ? para[0].Trim().ToUpper() : "";
            if (!(new List<string>() { "SIZE", "COUNT", "AGE", "TRACE" }).Contains(mode))
            {
                this._fatalException = new ArgumentException(this.syntax(String.Format("{0}: Es muss ein Arbeitsmodus angegeben werden.", this.GetType().Name)));
                throw this._fatalException;
            }
            string path = para.Length > 1 ? para[1].Trim() : "";
            if (String.IsNullOrEmpty(path))
            {
                this._fatalException = new ArgumentException(this.syntax(String.Format("{0}: Es muss ein Pfad angegeben werden.", this.GetType().Name)));
                throw this._fatalException;
            }
            string? dir = Path.GetDirectoryName(path);
            string fileMask = path.Replace(dir ?? "", "").TrimStart(Path.DirectorySeparatorChar);
            while (!String.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                dir = Path.GetDirectoryName(dir);
                if (String.IsNullOrEmpty(dir))
                {
                    dir = Path.GetDirectoryName(path);
                    fileMask = path.Replace(dir ?? "", "").TrimStart(Path.DirectorySeparatorChar);
                    if (!this._exceptionToFalse)
                    {
                        this._fatalException = new DirectoryNotFoundException(this.syntax(String.Format("{0}: Der Pfad wurde nicht gefunden: {1}.", this.GetType().Name, path)));
                        throw this._fatalException;
                    }
                    break;
                }
                // fileMask = path.Replace(dir, "").TrimStart(Path.DirectorySeparatorChar); // Geht nicht wegen regulärer Ausdrücke, die mit "\" beginnen.
                if (String.IsNullOrEmpty(dir))
                {
                    fileMask = path;
                }
                else
                {
                    fileMask = path.Replace(dir, "");
                }
                if (fileMask.StartsWith(@"\"))
                {
                    fileMask = fileMask.Substring(1);
                }
            }
            bool failIfNotFound = false;
            string comparer = para.Length > 2 ? para[2].Trim() : "";
            if (comparer.EndsWith("!"))
            {
                failIfNotFound = true;
                comparer = comparer.TrimEnd('!');
            }
            if (!Directory.Exists(dir))
            {
                //if (failIfNotFound)
                //{
                    this._fatalException = new FileNotFoundException(
                        String.Format("Das Verzeichnis '{0}' wurde nicht gefunden.", dir));
                    throw this._fatalException;
                //}

            }
            if (comparer != "<" && comparer != ">")
            {
                this._fatalException = new ArgumentException(this.syntax(String.Format("{0}: Es muss ein Vergleichsoperator < oder > angegeben werden.", this.GetType().Name)));
                throw this._fatalException;
            }
            string valueString = para.Length > 3 ? para[3].Trim() : "";
            long criticalFileSizeOrCount = -1;
            TimeSpan criticalFileAge = TimeSpan.MinValue;
            if (!long.TryParse(valueString, out criticalFileSizeOrCount))
            {
                TimeSpan span = TimeSpan.Zero;
                foreach (Match n in new Regex(@"[DHMS]\:\d+", RegexOptions.IgnoreCase).Matches(valueString))
                {
                    string timePart = n.Groups[0].Value;
                    string unity = (timePart + ":").Split(':')[0].ToUpper();
                    if (!(new string[] { "S", "M", "H", "D" }).Contains(unity))
                    {
                        this._fatalException = new ArgumentException(this.syntax(String.Format("{0}: Es wurde keine gültige Einheit (S, M, H, D) angegeben.", this.GetType().Name)));
                        throw this._fatalException;
                    }
                    long value = -1;
                    if (!long.TryParse((timePart + ":").Split(':')[1], out value))
                    {
                        this._fatalException = new ArgumentException(this.syntax(String.Format("{0}: Es wurde kein gültiger Wert für {1} angegeben.", this.GetType().Name, unity)));
                        throw this._fatalException;
                    }
                    switch (unity)
                    {
                        case "S":
                            span += TimeSpan.FromSeconds(value);
                            break;
                        case "M":
                            span += TimeSpan.FromMinutes(value);
                            break;
                        case "H":
                            span += TimeSpan.FromHours(value);
                            break;
                        case "D":
                            span += TimeSpan.FromDays(value);
                            break;
                    }
                }
                criticalFileAge = span;
            }
            string comment = para.Length > 4 ? para[4].Trim() : "";
            this._fileCheckerReturnObject = new FileCheckerReturnObject()
            {
                Mode = mode,
                SearchDir = dir,
                FileMask = fileMask,
                Comparer = comparer,
                CriticalFileSizeOrCount = criticalFileSizeOrCount,
                CriticalFileAge = criticalFileAge,
                FailIfNotFound = failIfNotFound,
                Comment = comment
            };
        }

        private bool filesToResult(List<string> files)
        {
            bool rtn = true;
            int filesCount = files != null ? files.Count : 0;
            if (files != null)
            {
                FileInfo fileInfo;
                DateTime lastWriteTime;
                long fileSize;
                TimeSpan datediff;
                foreach (string file in files)
                {
                    try
                    {
                        fileInfo = new FileInfo(file);
                        lastWriteTime = fileInfo.LastWriteTime;
                        fileSize = fileInfo.Length;
                        datediff = DateTime.Now - fileInfo.LastWriteTime;
                    }
                    catch
                    {
                        // sollte die Datei gerade im Try-Block wieder verschwinden (umbenannt werden),
                        // dann ist alles ok, also keine Exception und zur nächsten Datei weitergehen.
                        continue;
                    }
                    bool fileRtn = true;
                    switch (this._fileCheckerReturnObject.Mode)
                    {
                        case "COUNT":
                            break;
                        case "SIZE":
                            if (this._fileCheckerReturnObject.Comparer == ">" && fileSize <= this._fileCheckerReturnObject.CriticalFileSizeOrCount
                                || this._fileCheckerReturnObject.Comparer == "<" && fileSize >= this._fileCheckerReturnObject.CriticalFileSizeOrCount
                                || this._fileCheckerReturnObject.Comparer == "=" && fileSize != this._fileCheckerReturnObject.CriticalFileSizeOrCount)
                            {
                                fileRtn = false;
                            }
                            break;
                        case "AGE":
                            if (this._fileCheckerReturnObject.Comparer == ">" && datediff <= this._fileCheckerReturnObject.CriticalFileAge
                                || this._fileCheckerReturnObject.Comparer == "<" && datediff >= this._fileCheckerReturnObject.CriticalFileAge
                                || this._fileCheckerReturnObject.Comparer == "=" && datediff != this._fileCheckerReturnObject.CriticalFileAge)
                            {
                                fileRtn = false;
                            }
                            break;
                        case "TRACE":
                            if (this._tracedFiles.ContainsKey(file))
                            {
                                datediff = DateTime.Now - this._tracedFiles[file].Inserted;
                                if (this._fileCheckerReturnObject.Comparer == ">" && datediff <= this._fileCheckerReturnObject.CriticalFileAge
                                    || this._fileCheckerReturnObject.Comparer == "<" && datediff >= this._fileCheckerReturnObject.CriticalFileAge
                                    || this._fileCheckerReturnObject.Comparer == "=" && datediff != this._fileCheckerReturnObject.CriticalFileAge)
                                {
                                    fileRtn = false;
                                }
                            }
                            else
                            {
                                this._tracedFiles.Add(file, new TracedFile() { Inserted = DateTime.Now, Info = fileInfo });
                            }
                            break;
                        default:
                            break;
                    }
                    this._fileCheckerReturnObject.SubResults?.SubResults?.Add(new FileCheckerReturnObject.SubResult()
                    {
                        LogicalResult = fileRtn,
                        FileName = Path.GetFileName(file),
                        FileAge = datediff,
                        FileSize = fileInfo.Length
                    });
                    if (!fileRtn)
                    {
                        rtn = false;
                    }
                }
            }
            if (this._fileCheckerReturnObject.Mode == "COUNT")
            {
                if (this._fileCheckerReturnObject.Comparer == ">" && filesCount <= this._fileCheckerReturnObject.CriticalFileSizeOrCount
                    || this._fileCheckerReturnObject.Comparer == "<" && filesCount >= this._fileCheckerReturnObject.CriticalFileSizeOrCount
                    || this._fileCheckerReturnObject.Comparer == "=" && filesCount != this._fileCheckerReturnObject.CriticalFileSizeOrCount)
                {
                    rtn = false;
                }
            }

            if (files == null || files.Count == 0)
            {
                if (this._fileCheckerReturnObject.Mode == "AGE" || this._fileCheckerReturnObject.Mode == "SIZE")
                {
                    if (this._fileCheckerReturnObject.Comparer != "<" || this._fileCheckerReturnObject.FailIfNotFound == true)
                    {
                        rtn = false;
                    }
                    else
                    {
                        rtn = true;
                    }
                }
            }
            return rtn;
        }

        private string syntax(string errorMessage)
        {
            return (
                       errorMessage
                       + Environment.NewLine
                       + "Parameter: Modus|Dateipfad(RegEx im Dateinamen möglich)|<, = oder >|Größe, Anzahl oder Alter|Beschreibung"
                       + Environment.NewLine
                       + String.Format("Modus: SIZE=Dateigröße, COUNT=Datei-Anzahl, AGE=Zeit seit letzter Änderung, TRACE=Zeit der Überwachung durch {0}", this.GetType().Name)
                       + Environment.NewLine
                       + "Format von Alter: Einheit + ':' + ganzzahliger Wert"
                       + Environment.NewLine
                       + "Einheit: S=Sekunden, M=Minuten, H=Stunden, D=Tage."
                       + Environment.NewLine
                       + @"Beispiel: TRACE|\\server\Pfad\.*\.ext|<|M:10|Prüft, dass nichts länger als 10 Minuten nicht durchläuft."
                   );
        }

        private void OnNodeProgressChanged(int progressPercentage)
        {
            if (NodeProgressChanged != null)
            {
                NodeProgressChanged(null, new ProgressChangedEventArgs(progressPercentage, null));
            }
        }

    }
}
