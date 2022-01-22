using System.ComponentModel;
using System.Net.Http.Headers;

namespace TimePlanner.Domain.Utils
{
  /// <summary>
  /// Stores the T value and knows whether it is consistent. Also contains information
  /// about error or info messages that have occurred while obtaining the value T.
  /// </summary>
  /// <remarks>
  /// Use this interface to separate exception handling and validation logic
  /// from the domain flow.
  /// Make your functions return Result of T, and chain the results together
  /// using Bind extension method of the <see cref="Result" /> class.
  /// Then you can check the final result for success or failure,
  /// access its value and read error (or info) messages.
  /// </remarks>
  public interface IResult<out TValue, out TError>
  {
    TValue Value { get; }
    bool IsSuccess { get; }
    TError Error { get; }
  }

  public interface IVoidResult<out TError> : IResult<object, TError>
  {
  }

  /// <summary>
  /// Contains static methods related to <see cref="IResult{TValue, TError}" />
  /// </summary>
  public static class Result
  {
    private record ValueResult<T, E>
      (T Value, bool IsSuccess, E Error) : IResult<T, E>;

    private record VoidResult<T>(bool IsSuccess, T Error)
      : ValueResult<object, T>(new object(), IsSuccess, Error), IVoidResult<T>;

    /// <summary>
    /// Creates a <see cref="ValueResult{TValue,TError}" /> with a consistent Value of type T.
    /// </summary>
    public static IResult<TValue, TError> Success<TValue, TError>(TValue value)
    {
      return new ValueResult<TValue, TError>(value, true, default);
    }

    /// <summary>
    /// Creates a Result of type <see cref="object" /> that indicates a successful unit operation.
    /// </summary>
    public static IVoidResult<TError> Success<TError>()
    {
      return new VoidResult<TError>(true, default);
    }

    public static IVoidResult<TError> Failure<TError>(TError error)
    {
      return new VoidResult<TError>(false, error);
    }


    /// <summary>
    /// Creates a <see cref="ValueResult{TValue,TError}" /> with a default value of type T and
    /// a collection of messages containing the specified message.
    /// The IsSuccess property will return false.
    /// </summary>
    public static IResult<TValue, TError> Failure<TValue, TError>(TError message)
    {
      return new ValueResult<TValue, TError>(default, false, message);
    }

    /// <summary>
    /// Applies a function if result is Success.
    /// Merges any existing messages with the new result.
    /// </summary>
    /// <remarks>
    /// Use it to chain Result generating functions together.
    /// </remarks>
    public static IResult<TValueR, TError> Bind<TValue, TValueR, TError>(this IResult<TValue, TError> result,
      Func<TValue, IResult<TValueR, TError>> func)
    {
      if (result.IsSuccess)
      {
        var nextValueResult = func(result.Value);
        return nextValueResult.IsSuccess
          ? Success<TValueR, TError>(nextValueResult.Value)
          : Failure<TValueR, TError>(nextValueResult.Error);
      }

      return Failure<TValueR, TError>(result.Error);
    }

    public static IVoidResult<TError> Bind<TValue, TError>(this IResult<TValue, TError> result,
      Func<TValue, IVoidResult<TError>> func)
    {
      if (result.IsSuccess)
      {
        var nextValueResult = func(result.Value);
        return nextValueResult.IsSuccess
          ? Success<TError>()
          : Failure(nextValueResult.Error);
      }

      return Failure(result.Error);
    }

    public static IResult<TValue,TErrorR> MapError<TValue, TError, TErrorR>(this IResult<TValue, TError> result,
      Func<TError, TErrorR> errorFunc)
    {
      return result.IsSuccess
        ? Success<TValue,TErrorR>(result.Value)
        : Failure<TValue,TErrorR>(errorFunc(result.Error));
    }

    public static IVoidResult<TErrorR> MapError<TError, TErrorR>(this IVoidResult<TError> result,
      Func<TError, TErrorR> errorFunc)
    {
      return result.IsSuccess
        ? Success<TErrorR>()
        : Failure<TErrorR>(errorFunc(result.Error));
    }

    /// <summary>
    /// Applies a function if result is Success. Wraps the result into Result.
    /// </summary>
    /// <remarks>
    /// Use it to TValueRansform the Result's underlying type to another type.
    /// </remarks>
    public static IResult<TValueR, TError> Map<TValue, TValueR, TError>(this IResult<TValue, TError> result,
      Func<TValue, TValueR> func)
    {
      //logically it is the same as Lift with one argument
      //Map is added for convenience as an extension method to Result
      return Lift(func, result);
    }

    /// <summary>
    /// Invokes an Action if result is Success and passes the result through.
    /// </summary>
    /// <remarks>
    /// Use it when you don't care about the result of the method,
    /// for example, logging.
    /// </remarks>
    public static IResult<TValue, TError> Tee<TValue, TError>(this IResult<TValue, TError> result,
      Action<TValue> action)
    {
      if (result.IsSuccess)
      {
        action(result.Value);
      }

      return result;
    }

    public static IVoidResult<TError> Tee<TError>(this IVoidResult<TError> result,
      Action action)
    {
      if (result.IsSuccess)
      {
        action();
      }

      return result;
    }

    /// <summary>
    /// Given a function that transforms a value
    /// applies it only if the argument is Success.
    /// </summary>
    /// <remarks>
    /// Use it to ensure that an argument provided to the function is consistent.
    /// </remarks>
    public static IResult<TValueR, TError> Lift<TValue, TValueR, TError>(
      Func<TValue, TValueR> func,
      IResult<TValue, TError> arg)
    {
      if (arg.IsSuccess)
      {
        return Success<TValueR, TError>(func(arg.Value));
      }

      return Failure<TValueR, TError>(arg.Error);
    }

    /// <summary>
    /// Given a two parameters function
    /// applies it to arguments' values only if they both are Success.
    /// Merges any existing messages.
    /// </summary>
    /// <remarks>
    /// Use it to ensure that all arguments passed to the function are consistent,
    /// and if not, you will get a validation message from every argument that
    /// failed validation.
    /// </remarks>
    public static IResult<TValueR, IEnumerable<TError>> Lift<TValue, TValue1, TValueR, TError>(
      Func<TValue, TValue1, TValueR> func,
      IResult<TValue, TError> arg,
      IResult<TValue1, TError> arg1)
    {
      var messages = new[] { arg.Error, arg1.Error };
      if (arg.IsSuccess && arg1.IsSuccess)
      {
        return Success<TValueR, IEnumerable<TError>>(func(arg.Value, arg1.Value));
      }

      return Failure<TValueR, IEnumerable<TError>>(messages);
    }

    /// <summary>
    /// Given a three parameters function
    /// applies it to the arguments' values only if they all are Success.
    /// Merges any existing messages.
    /// </summary>
    /// ///
    /// <remarks>
    /// Use it to ensure that all arguments passed to the function are consistent,
    /// and if not, you will get a validation message from every argument that
    /// failed validation.
    /// </remarks>
    public static IResult<TValueR, IEnumerable<TError>> Lift<TValue, TValue1, TValue2, TValueR, TError>(
      Func<TValue, TValue1, TValue2, TValueR> func, IResult<TValue, TError> arg, IResult<TValue1, TError> arg1,
      IResult<TValue2, TError> arg2)
    {
      var messages = new[] { arg.Error, arg1.Error, arg2.Error };
      if (arg.IsSuccess && arg1.IsSuccess && arg2.IsSuccess)
      {
        return Success<TValueR, IEnumerable<TError>>(func(arg.Value, arg1.Value, arg2.Value));
      }

      return Failure<TValueR, IEnumerable<TError>>(messages);
    }

    /// <summary>
    /// Given a four parameters' function
    /// applies it to arguments' values only if they all are Success.
    /// Merges any existing messages.
    /// </summary>
    /// ///
    /// <remarks>
    /// Use it to ensure that all arguments passed to the function are consistent,
    /// and if not, you will get a validation message from every argument that
    /// failed validation.
    /// </remarks>
    public static IResult<TValueR, IEnumerable<TError>> Lift<TValue, TValue1, TValue2, TValue3, TValueR, TError>(
      Func<TValue, TValue1, TValue2, TValue3, TValueR> func,
      IResult<TValue, TError> arg,
      IResult<TValue1, TError> arg1,
      IResult<TValue2, TError> arg2,
      IResult<TValue3, TError> arg3)
    {
      var messages = new[] { arg.Error, arg1.Error, arg2.Error, arg3.Error };
      if (arg.IsSuccess && arg1.IsSuccess && arg2.IsSuccess && arg3.IsSuccess)
      {
        return Success<TValueR, IEnumerable<TError>>(func(arg.Value, arg1.Value, arg2.Value, arg3.Value));
      }

      return Failure<TValueR, IEnumerable<TError>>(messages);
    }
  }

}
