using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HttpService
{
    public enum MobileMessageType
    {
        SMS = 4,
        URL = 5,
        MMS = 6,
    }

    public enum FILETYPE
    {
        IMG,
        TXT,
        ADO,
        MOV,
    }


    public enum CRUD
    {
        list,
        C,
        R,
        U,
        D,
    }
}
