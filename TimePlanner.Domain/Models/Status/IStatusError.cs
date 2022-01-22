namespace TimePlanner.Domain.Models.Status
{
  public interface IStatusError
  {
    T Convert<T>(
      Func<TooManyWorkItems, T> funcOne,
      Func<WorkItemDoesNotExist, T> funcTwo,
      Func<DurationOverflow, T> funcThree,
      Func<UnexpectedError, T> funcFour);
  }

  public record TooManyWorkItems : IStatusError
  {
    T IStatusError.Convert<T>(
      Func<TooManyWorkItems, T> funcOne,
      Func<WorkItemDoesNotExist, T> funcTwo,
      Func<DurationOverflow, T> funcThree,
      Func<UnexpectedError, T> funcFour)
    {
      return funcOne(this);
    }
  }

  public record WorkItemDoesNotExist(int Index) : IStatusError
  {
    T IStatusError.Convert<T>(
      Func<TooManyWorkItems, T> funcOne,
      Func<WorkItemDoesNotExist, T> funcTwo,
      Func<DurationOverflow, T> funcThree,
      Func<UnexpectedError, T> funcFour)
    {
      return funcTwo(this);
    }
  }

  public record DurationOverflow(TimeSpan AvailableValue) : IStatusError
  {
    T IStatusError.Convert<T>(
      Func<TooManyWorkItems, T> funcOne,
      Func<WorkItemDoesNotExist, T> funcTwo,
      Func<DurationOverflow, T> funcThree,
      Func<UnexpectedError, T> funcFour)
    {
      return funcThree(this);
    }
  }

  public record UnexpectedError : IStatusError
  {
    T IStatusError.Convert<T>(
      Func<TooManyWorkItems, T> funcOne,
      Func<WorkItemDoesNotExist, T> funcTwo,
      Func<DurationOverflow, T> funcThree,
      Func<UnexpectedError, T> funcFour)
    {
      return funcFour(this);
    }
  }
}
