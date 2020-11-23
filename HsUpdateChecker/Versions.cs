using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace HsUpdateChecker
{
    public static class VersionsHelper
    {
        public static IEnumerable<VersionsInfo> Parse(Stream stream)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = "|",
                AllowComments = true,
                Comment = '#'
            };
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader, config))
            {
                return csv.GetRecords<VersionsInfo>().ToList();
            }
        }
    }
    public class VersionsInfo
    {
        #region Properties
        [Name("Region!STRING:0")]
        public string Region { get; set; }
        [Name("BuildConfig!HEX:16")]
        public byte[] BuildConfig { get; set; }
        [Name("CDNConfig!HEX:16")]
        public byte[] CDNConfig { get; set; }
        [Name("KeyRing!HEX:16")]
        public byte[] KeyRing { get; set; }
        [Name("BuildId!DEC:4")]
        public int BuildId { get; set; }
        [Name("VersionsName!String:0")]
        public string VersionsName { get; set; }
        [Name("ProductConfig!HEX:16")]
        public byte[] ProductConfig { get; set; }
        #endregion
    }
}
