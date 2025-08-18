namespace Api.Domain.Entities;

public enum PaperStatus : byte
{
    Uploaded = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
}
