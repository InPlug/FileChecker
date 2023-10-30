using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace FileChecker
{
    /// <summary>
    /// ReturnObject für Ergebnisse von File-Checkern.
    /// </summary>
    /// <remarks>
    /// File: FileCheckerReturnObject.cs
    /// Autor: Erik Nagel, NetEti
    ///
    /// 16.05.2015 Erik Nagel: erstellt
    /// </remarks>
    [DataContract]//[Serializable()]
    public class FileCheckerReturnObject
    {
        /// <summary>
        /// Wrapper-Klasse um List&lt;SubResult&gt; SubResults.
        /// </summary>
        [DataContract]//[Serializable()]
        public class SubResultListContainer
        {
            /// <summary>
            /// Bis zu drei KeyValue-Paare mit jeweils DetailName (i.d.R "Anzahl"),
            /// und einem KeyValue-Paar bestehend aus DetailValue (i.d.R eine int-Anzahl)
            /// und einem Detail-Ergebnis (bool?).
            /// </summary>
            [DataMember]
            public List<SubResult>? SubResults { get; set; }

            /// <summary>
            /// Standard Konstruktor.
            /// </summary>
            public SubResultListContainer()
            {
                this.SubResults = new List<SubResult>();
            }

            /// <summary>
            /// Deserialisierungs-Konstruktor.
            /// </summary>
            /// <param name="info">Property-Container.</param>
            /// <param name="context">Übertragungs-Kontext.</param>
            protected SubResultListContainer(SerializationInfo info, StreamingContext context)
            {
                this.SubResults = (List<SubResult>?)info.GetValue("SubResults", typeof(List<SubResult>));
            }

            /// <summary>
            /// Serialisierungs-Hilfsroutine: holt die Objekt-Properties in den Property-Container.
            /// </summary>
            /// <param name="info">Property-Container.</param>
            /// <param name="context">Serialisierungs-Kontext.</param>
            public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("SubResults", this.SubResults);
            }

            /// <summary>
            /// Überschriebene ToString()-Methode.
            /// </summary>
            /// <returns>Dieses Objekt.ToString()</returns>
            public override string ToString()
            {
                StringBuilder stringBuilder = new StringBuilder();
                string delimiter = "";
                if (this.SubResults != null)
                {
                    foreach (SubResult subResult in this.SubResults)
                    {
                        stringBuilder.Append(delimiter + subResult.ToString());
                        delimiter = Environment.NewLine;
                    }
                }
                return stringBuilder.ToString();
            }

            /// <summary>
            /// Vergleicht dieses Objekt mit einem übergebenen Objekt nach Inhalt.
            /// </summary>
            /// <param name="obj"></param>
            /// <returns>True, wenn der übergebene SubResultListContainer inhaltlich gleich diesem SubResultListContainer ist.</returns>
            public override bool Equals(object? obj)
            {
                if (obj == null || this.GetType() != obj.GetType())
                {
                    return false;
                }
                if (Object.ReferenceEquals(this, obj))
                {
                    return true;
                }
                SubResultListContainer subResultList = (SubResultListContainer)obj;
                if (this.SubResults?.Count != subResultList.SubResults?.Count)
                {
                    return false;
                }
                for (int i = 0; i < this.SubResults?.Count; i++)
                {
                    if (this.SubResults[i] != subResultList.SubResults?[i])
                    {
                        return false;
                    }
                }
                return true;
            }

            /// <summary>
            /// Erzeugt einen eindeutigen Hashcode für dieses Objekt.
            /// </summary>
            /// <returns>Hashcode (int).</returns>
            public override int GetHashCode()
            {
                return (this.ToString()).GetHashCode();
            }
        }

        /// <summary>
        /// Klasse für ein Teilergebnis.
        /// </summary>
        [DataContract]//[Serializable()]
        public class SubResult
        {
            /// <summary>
            /// Das logische Einzelergebnis eines Unterergebnisses.
            /// true, false oder null.
            /// </summary>
            [DataMember]
            public bool? LogicalResult { get; set; }

            /// <summary>
            /// Der Name einer gelisteten Datei.
            /// (i.d.R "Anzahl").
            /// </summary>
            [DataMember]
            public string? FileName { get; set; }

            /// <summary>
            /// Die Größe einer Datei.
            ///  </summary>
            [DataMember]
            public long? FileSize { get; set; }

            /// <summary>
            /// Die Zeitspanne seit der letzten Änderung einer Datei oder
            /// die Dauer der Überwachung dieser Datei (Trace = true).
            ///  </summary>
            [DataMember]
            public TimeSpan? FileAge { get; set; }

            /// <summary>
            /// Standard Konstruktor.
            /// </summary>
            public SubResult() { }

            /// <summary>
            /// Deserialisierungs-Konstruktor.
            /// </summary>
            /// <param name="info">Property-Container.</param>
            /// <param name="context">Übertragungs-Kontext.</param>
            protected SubResult(SerializationInfo info, StreamingContext context)
            {
                this.LogicalResult = (bool?)info.GetValue("LogicalResult", typeof(bool?));
                this.FileName = info.GetString("FileName");
                this.FileSize = (long?)info.GetValue("FileSize", typeof(long));
                this.FileAge = (TimeSpan?)info.GetValue("FileAge", typeof(TimeSpan));
            }

            /// <summary>
            /// Serialisierungs-Hilfsroutine: holt die Objekt-Properties in den Property-Container.
            /// </summary>
            /// <param name="info">Property-Container.</param>
            /// <param name="context">Serialisierungs-Kontext.</param>
            public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("LogicalResult", this.LogicalResult);
                info.AddValue("FileName", this.FileName);
                info.AddValue("FileSize", this.FileSize);
                info.AddValue("FileAge", this.FileAge);
            }

            /// <summary>
            /// Überschriebene ToString()-Methode.
            /// </summary>
            /// <returns>Dieses Objekt.ToString()</returns>
            public override string ToString()
            {
                string resultStr = this.LogicalResult == null ? "null" : this.LogicalResult.ToString() ?? "";
                return String.Format("{0}, {1}: {2}, {3}", resultStr, this.FileName, this.FileSize.ToString(),
                        this.FileAge?.ToString(@"d\d\:h\h\:m\m\:s\s", System.Globalization.CultureInfo.InvariantCulture));
            }

            /// <summary>
            /// Vergleicht dieses Objekt mit einem übergebenen Objekt nach Inhalt.
            /// </summary>
            /// <param name="obj">Das zu vergleichende SubResult.</param>
            /// <returns>True, wenn das übergebene SubResult inhaltlich gleich diesem SubResult ist.</returns>
            public override bool Equals(object? obj)
            {
                if (obj == null || this.GetType() != obj.GetType())
                {
                    return false;
                }
                if (Object.ReferenceEquals(this, obj))
                {
                    return true;
                }
                if (this.ToString() != obj.ToString())
                {
                    return false;
                }
                return true;
            }

            /// <summary>
            /// Erzeugt einen eindeutigen Hashcode für dieses Objekt.
            /// </summary>
            /// <returns>Hashcode (int).</returns>
            public override int GetHashCode()
            {
                return (this.ToString()).GetHashCode();
            }
        }

        /// <summary>
        /// Wrapper-Klasse um List&lt;SubResult&gt; SubResults.
        /// </summary>
        [DataMember]
        public SubResultListContainer? SubResults { get; set; }

        /// <summary>
        /// Das logische Gesamtergebnis eines Prüfprozesses:
        /// true, false oder null.
        /// </summary>
        [DataMember]
        public bool? LogicalResult { get; set; }

        /// <summary>
        /// Arbeits-Modus:
        /// SIZE=Dateigröße, COUNT=Datei-Anzahl, AGE=Zeit seit letzter Änderung, TRACE=Zeit der Überwachung.
        /// </summary>
        [DataMember]
        public string? Mode { get; set; }

        /// <summary>
        /// Die Anzahl der Dateien, die das
        /// Prüfkriterium erfüllen.
        /// </summary>
        [DataMember]
        public int? CountFiles { get; set; }

        /// <summary>
        /// Das Verzeichnis aus dem übergebenen Suchpfad. 
        ///  </summary>
        [DataMember]
        public string? SearchDir { get; set; }

        /// <summary>
        /// Der Dateiname aus dem übergebenen Suchpfad. 
        /// Der Dateiname kann ein regulärer Ausdruck sein.
        ///  </summary>
        [DataMember]
        public string? FileMask { get; set; }

        /// <summary>
        /// Übergebener Vergleichsoperator (&lt; oder &gt;).
        /// </summary>
        [DataMember]
        public string? Comparer { get; set; }

        /// <summary>
        /// Größe einer Datei oder Anzahl Dateien, bei deren Überschreitung
        /// oder Unterschreitung (je nach Comparer) das Ergebnis der Routine
        /// auf false geht.
        /// </summary>
        [DataMember]
        public long? CriticalFileSizeOrCount { get; set; }

        /// <summary>
        /// Maximales oder minimales Alter der gefundenen Dateien
        /// (je nach Comparer) bei dem das Ergebnis der Routine auf false geht.
        ///  </summary>
        [DataMember]
        public TimeSpan? CriticalFileAge { get; set; }

        /// <summary>
        /// Bei true wird, wenn die zu untersuchende(n) Datei(en) nicht gefunden wurden,
        /// als Ergebnis der Prüfung immer false zurückgegeben, auch wenn der Modus
        /// "AGE" oder "SIZE" und der Vergleichsoperator "&lt;" ist.
        /// </summary>
        [DataMember]
        public bool? FailIfNotFound { get; set; }

        /// <summary>
        /// Klartext-Informationen zur Prüfroutine
        /// (was die Routine prüft).
        ///  </summary>
        [DataMember]
        public string? Comment { get; set; }

        /// <summary>
        /// Standard Konstruktor.
        /// </summary>
        public FileCheckerReturnObject()
        {
            this.SubResults = new SubResultListContainer();
            this.LogicalResult = null;
            this.CountFiles = 0;
        }

        /// <summary>
        /// Deserialisierungs-Konstruktor.
        /// </summary>
        /// <param name="info">Property-Container.</param>
        /// <param name="context">Übertragungs-Kontext.</param>
        protected FileCheckerReturnObject(SerializationInfo info, StreamingContext context)
        {
            this.SubResults = (SubResultListContainer?)info.GetValue("SubResults", typeof(SubResultListContainer));
            this.LogicalResult = (bool?)info.GetValue("LogicalResult", typeof(bool?));
            this.Mode = info.GetString("Mode");
            this.CountFiles = (int?)info.GetValue("CountFiles", typeof(int));
            this.SearchDir = info.GetString("SearchDir");
            this.FileMask = info.GetString("FileMask");
            this.Comparer = info.GetString("Comparer");
            this.CriticalFileSizeOrCount = (long?)info.GetValue("CriticalFileSizeOrCount", typeof(long));
            this.CriticalFileAge = (TimeSpan?)info.GetValue("CriticalFileAge", typeof(TimeSpan));
            this.Comment = info.GetString("Comment");
        }

        /// <summary>
        /// Serialisierungs-Hilfsroutine: holt die Objekt-Properties in den Property-Container.
        /// </summary>
        /// <param name="info">Property-Container.</param>
        /// <param name="context">Serialisierungs-Kontext.</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("SubResults", this.SubResults);
            info.AddValue("LogicalResult", this.LogicalResult);
            info.AddValue("Mode", this.Mode);
            info.AddValue("CountFiles", this.CountFiles);
            info.AddValue("SearchDir", this.SearchDir);
            info.AddValue("FileMask", this.FileMask);
            info.AddValue("Comparer", this.Comparer);
            info.AddValue("CriticalFileSizeOrCount", this.CriticalFileSizeOrCount);
            info.AddValue("CriticalFileAge", this.CriticalFileAge);
            info.AddValue("Comment", this.Comment);
        }

        /// <summary>
        /// Überschriebene ToString()-Methode - stellt alle öffentlichen Properties
        /// als einen (mehrzeiligen) aufbereiteten String zur Verfügung.
        /// </summary>
        /// <returns>Alle öffentlichen Properties als ein String aufbereitet.</returns>
        public override string ToString()
        {
            string logicalResultStr = this.LogicalResult?.ToString() ?? "";
            StringBuilder str = new StringBuilder(String.Format("{0} ({1})", logicalResultStr == "" ? "null" : logicalResultStr, this.Comment));
            str.Append(String.Format("\n\nModus {0}", this.Mode));
            str.Append(String.Format("\nPfad {0}+{1} {2} {3}", this.SearchDir, this.FileMask, this.Comparer,
              this.CriticalFileSizeOrCount < 0 ? this.CriticalFileAge.ToString() : this.CriticalFileSizeOrCount.ToString()));
            str.Append(String.Format("\nCountFiles {0}", this.CountFiles.ToString()));
            if (this.SubResults?.SubResults != null)
            {
                foreach (SubResult subResult in this.SubResults.SubResults)
                {
                    str.Append(String.Format("\n    {0}", subResult.ToString()));
                }
            }
            return str.ToString();
        }

        /// <summary>
        /// Vergleicht dieses Objekt mit einem übergebenen Objekt nach Inhalt.
        /// </summary>
        /// <param name="obj">Das zu vergleichende FileCheckerReturnObject.</param>
        /// <returns>True, wenn das übergebene FileCheckerReturnObject inhaltlich gleich diesem FileCheckerReturnObject ist.</returns>
        public override bool Equals(object? obj)
        {
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }
            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }
            if (this.ToString() != obj.ToString())
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Erzeugt einen eindeutigen Hashcode für dieses Objekt.
        /// </summary>
        /// <returns>Hashcode (int).</returns>
        public override int GetHashCode()
        {
            return (this.ToString()).GetHashCode();
        }
    }
}
