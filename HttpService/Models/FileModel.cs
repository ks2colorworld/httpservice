using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HttpService.Models
{
    public class FileModel
    {
        public string Name { get; set; }
        public string Format { get; set; }
        public long Size { get; set; }

        public static FileModel Empty
        {
            get => new FileModel();
        }
    }
}
