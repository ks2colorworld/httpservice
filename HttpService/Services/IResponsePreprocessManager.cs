using HttpService.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace HttpService.Services
{
    public interface IResponsePreprocessManager
    {
        ResponseModel ProcessDataSet(DataSet dataSet);

        ResponseModel ProcessException(Exception ex);

        ResponseModel ProcessFile(string file);
    }
}
