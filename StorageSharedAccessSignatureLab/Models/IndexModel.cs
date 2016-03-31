using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StorageSharedAccessSignatureLab.Models
{
    public class IndexModel
    {
        public string ContainerUrl { get; set; }
        public string SasToken { get; set; }
    }
}