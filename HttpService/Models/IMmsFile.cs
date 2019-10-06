using HttpService.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HttpService.Models
{
    public interface IMmsFile
    {
        FILETYPE FileType { get; }

        string FileLocalPath { get; }

        string FileDownloadUrl { get; }

        string FileName { get; }
    }

    public abstract class MmsFileBase : IMmsFile
    {
        public MmsFileBase(string fileType)
        {
            this.fileType = fileType;
        }

        public FILETYPE FileType { get => ConvertFileType(); }

        public abstract string FileLocalPath { get; }

        public abstract string FileDownloadUrl { get; }

        public abstract string FileName { get; }

        private FILETYPE ConvertFileType()
        {
            if (String.IsNullOrEmpty(fileType))
            {
                throw new ArgumentException("파일 형식은 빈값을 허용하지 않습니다.", nameof(fileType));
            }
            FILETYPE result = FILETYPE.TXT;

            if (!Enum.TryParse<FILETYPE>(fileType, out result))
            {
                throw new ArgumentException("파일 형식이 유효하지 않습니다.", nameof(fileType));
            }

            return result;
        }

        protected string fileType;
    }

    public class RemoteMmsFie : MmsFileBase
    {
        public RemoteMmsFie(
            string fileType,
            string fileDownloadBasicUrl,
            string file_basic_local_path,
            string fileName)
            : base(fileType)
        {
            this.file_download_basic_url = fileDownloadBasicUrl;
            this.file_name = fileName;
            //this.file_basic_local_path = @"c:\temp\";
            this.file_basic_local_path = file_basic_local_path;
        }

        public override string FileLocalPath
        {
            get => System.IO.Path.Join(file_basic_local_path, file_name);
        }

        public override string FileDownloadUrl
        {
            get => $"{file_download_basic_url}{file_name}";
        }

        public override string FileName
        {
            get => file_name;
        }

        private string file_basic_local_path;
        private string file_download_basic_url;
        private string file_name;
    }

    public class LocalMmsFile : MmsFileBase
    {
        public LocalMmsFile(string fileType, string filePath)
              : base(fileType)
        {
            this.filePath = filePath;

            fileInfo = new System.IO.FileInfo(filePath);
        }

        public override string FileLocalPath { get => filePath; }
        public override string FileDownloadUrl { get => throw new NotSupportedException(); }
        public override string FileName { get => fileInfo.Name; }

        private readonly string filePath;
        private readonly System.IO.FileInfo fileInfo;
    }
}
