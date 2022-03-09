namespace TimePlanner.Domain.Services.Interfaces
{
  public class DataAccessException : ApplicationException
  {
    public DataAccessException(string message) : base(message)
    {
    }
  }
}
