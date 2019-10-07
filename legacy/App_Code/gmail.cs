using System;
using System.Net.Mail;
using System.Collections;
using System.IO.Compression;
using Ionic.Utils.Zip;
using System.IO;
using System.Runtime.InteropServices;

//Developed By: Daniel J. C. Costa;
//WebSite: http://www.danielcosta.pt
//Blog: http://blog.danielcosta.pt
//E-Mail: gmailsend@danielcosta.pt


namespace GmailSend
{
    public class gmail
    {
        private string _title;
        private string _message;
        private int _priority;
        private string _username;
        private string _password;
        private Boolean _autenticado = false;
        private ArrayList _listTo = new ArrayList();
        private ArrayList _listCc = new ArrayList();
        private ArrayList _listBcc = new ArrayList();
        private ArrayList _listAnexos = new ArrayList();
        private string _zipFileName;
        private Boolean _html = false;
        private string _fromAlias = "";

        
        // Limpeza de memória e de ficheiros usada pela DLL (Que limpinha....)
        [STAThread]
        [DllImport("KERNEL32.DLL", EntryPoint = "SetProcessWorkingSetSize", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool SetProcessWorkingSetSize(IntPtr pProcess, int dwMinimumWorkingSetSize, int dwMaximumWorkingSetSize);

        [DllImport("KERNEL32.DLL", EntryPoint = "GetCurrentProcess", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetCurrentProcess();

        private void LimpaRecursos()
        {
            if (File.Exists(_zipFileName))
            {
                try
                {
                    File.Delete(_zipFileName);
                }
                catch
                { }
                _zipFileName = "";
            }

            IntPtr pHandle = GetCurrentProcess();
            SetProcessWorkingSetSize(pHandle, -1, -1);
        }
        // Fim da limpeza


        public string fromAlias
        {
            set { _fromAlias = value; }
        }
        public string To
        {
            set { _listTo.Add(value); }
        }
        public string Cc
        {
            set { _listCc.Add(value); }
        }
        public string Bcc
        {
            set { _listBcc.Add(value); }
        }
        public string Subject
        {
            get { return this._title; }
            set { this._title = value; }
        }
        public string Message
        {
            get { return this._message; }
            set { this._message = value; }
        }
        public int Priority
        {
            get { return this._priority; }
            set { this._priority = value; }
        }
        public Boolean Html
        {
            get { return this._html; }
            set { this._html = value; }
        }
        public gmail() {
            
        }

        SmtpClient mailClient = new SmtpClient("smtp.gmail.com", 587);
        
        private void autentica()
        {
            System.Net.NetworkCredential mailAuthentication = new
            System.Net.NetworkCredential(this._username, this._password);
            mailClient.EnableSsl = true;
            mailClient.UseDefaultCredentials = false;
            mailClient.Credentials = mailAuthentication;
            this._autenticado = true;
        }

        public Boolean auth(string username, string password)
        {
            this._username = username;
            this._password = password;
            autentica();
            return true;
        }
        public Boolean send()
        {
            if (_autenticado == true)
            {
                MailMessage MyMailMessage = new MailMessage();
                if (_fromAlias == ""){
                    MyMailMessage.From = new MailAddress(_username);
                }
                else
                {
                    MyMailMessage.From = new MailAddress(this._username, this._fromAlias);

                }
                
                MyMailMessage.Subject = this._title;
                MyMailMessage.Body = this._message;
                MyMailMessage.IsBodyHtml = this._html;


                if (this._priority == 0)
                {
                    MyMailMessage.Priority = MailPriority.Normal;
                }
                else if (this._priority == 1)
                {
                    MyMailMessage.Priority = MailPriority.Low;
                }
                else if (this._priority == 2)
                {
                    MyMailMessage.Priority = MailPriority.High;
                }

                for (int i = 0; i < _listAnexos.Count; i++)
                {
                    Attachment mailatt = new Attachment(_listAnexos[i].ToString());
                    MyMailMessage.Attachments.Add(mailatt);
                    
                }
                for (int i = 0; i < _listTo.Count; i++)
                {
                    MailAddress destino = new MailAddress(_listTo[i].ToString());
                    MyMailMessage.To.Add(destino);
                }
                for (int i = 0; i < _listCc.Count; i++)
                {
                    MailAddress conhecimento = new MailAddress(_listCc[i].ToString());
                    MyMailMessage.CC.Add(conhecimento);
                }
                for (int i = 0; i < _listBcc.Count; i++)
                {
                    MailAddress escondido = new MailAddress(_listBcc[i].ToString());
                    MyMailMessage.Bcc.Add(escondido);
                }

                mailClient.Send(MyMailMessage);
                LimpaRecursos();
                return true;
            }
            else
            {
                return false;
            }
        }

        public void attach(string file)
        {
            _listAnexos.Add(file);
        }

        public void zip(string Name, string Password)
        {
            
            if (File.Exists(Name))
            {
                try
                {
                    File.Delete(Name);
                }
                catch
                {
                    Name = "MailAttach_" + DateTime.Today.Second.ToString() + Name;
                }
                _zipFileName = Name;
            }
            using (ZipFile zip = new ZipFile(Name))
            {
                if (Password + "" != "")
                {
                    zip.Password = Password;
                }

                for (int i = 0; i < _listAnexos.Count; i++)
                {
                    zip.AddFile(_listAnexos[i].ToString());
                }
                zip.Save();

                _listAnexos.Clear();
                _listAnexos.Add(Name);
            }
        }
    }
}
