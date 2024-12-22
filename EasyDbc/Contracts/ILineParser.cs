namespace EasyDbc.Contracts
{
    internal interface ILineParser
    {
        bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider);
    }
}