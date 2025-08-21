using CurrencyConverter.Common;
using System.Net;

namespace CurrencyConverter.Business.Dto;

public class ResponseDto<T>
{
    public bool Success { get; private set; }
    public T Result { get; set; }
    public string Message { get; set; }

    //TODO y its here??? can same be done using HttpReturn methods???
    public HttpStatusCode StatusCode { get; set; }
    public string Exception { get; set; }

    public List<string> Errors { get; set; } = new();
    public ResponseDto()
    {
        Success = true;
        Message = AppResources.Success;
        StatusCode = HttpStatusCode.OK;
    }
    public void AddError(List<string> errorList)
    {
        Errors.AddRange(errorList);
        Success = false;
        Message = AppResources.Failure;
    }
    public void AddError(string error)
    {
        Errors.Add(error);
        Success = false;
        Message = AppResources.Failure;
    }
}
