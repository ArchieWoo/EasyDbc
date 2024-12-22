namespace EasyDbc.Contracts
{
    public interface INextLineProvider
    {
        bool TryGetLine(out string line);
    }
}
