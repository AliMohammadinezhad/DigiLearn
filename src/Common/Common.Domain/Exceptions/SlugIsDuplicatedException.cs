namespace Common.Domain.Exceptions;

public class SlugIsDuplicatedException : BaseDomainException
{
    public SlugIsDuplicatedException() : base("اسلاگ تکراری است")
    {
    }

    public SlugIsDuplicatedException(string message) : base(message)
    {
    }

}