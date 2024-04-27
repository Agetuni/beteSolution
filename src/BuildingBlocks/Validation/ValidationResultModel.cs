using FluentValidation.Results;
using System.Net;
using System.Text.Json;

namespace BuildingBlocks.Validation;

public class ValidationResultModel
{
    public int StatusCode { get; set; } = (int)HttpStatusCode.BadRequest;
    public string Message { get; set; } = "Validation Failed.";

    public List<ValidationFailure> Errors { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}