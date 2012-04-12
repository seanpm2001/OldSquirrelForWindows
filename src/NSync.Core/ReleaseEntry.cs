﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace NSync.Core
{
    public class ReleaseEntry : IEnableLogger
    {
        public string SHA1 { get; protected set; }
        public string Filename { get; protected set; }
        public long Filesize { get; protected set; }
        public bool IsDelta { get; protected set; }

        protected ReleaseEntry(string sha1, string filename, long filesize, bool isDelta)
        {
            SHA1 = sha1; Filename = filename; Filesize = filesize; IsDelta = isDelta;
        }

        static readonly Regex entryRegex = new Regex(@"^([0-9a-fA-F]{40})\s+(\S+)\s+(\d+)$");
        public static ReleaseEntry ParseReleaseEntry(string entry)
        {
            var m = entryRegex.Match(entry);
            if (!m.Success) {
                return null;
            }

            if (m.Groups.Count != 4) {
                return null;
            }

            long size = Int64.Parse(m.Groups[3].Value);
            bool isDelta = filenameIsDeltaFile(m.Groups[2].Value);
            return new ReleaseEntry(m.Groups[1].Value, m.Groups[2].Value, size, isDelta);
        }

        public string EntryAsString {
            get { return String.Format("{0} {1} {2}", SHA1, Filename, Filesize); } 
        }

        public static ReleaseEntry GenerateFromFile(Stream file, string filename)
        {
            var sha1 = System.Security.Cryptography.SHA1.Create();

            var hash = BitConverter.ToString(sha1.ComputeHash(file)).Replace("-", String.Empty); 
            return new ReleaseEntry(hash, filename, file.Length, filenameIsDeltaFile(filename));
        }

        static bool filenameIsDeltaFile(string filename)
        {
            return filename.EndsWith(".delta", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}