using AutoFixture;
using NUnit.Framework;
using TimePlanner.Domain.Utils;

namespace TimePlanner.Domain.UnitTests.Utils
{
  /// <summary>
  /// Tests for <see cref="IResult{ TValue, TError }" />
  /// </summary>
  public class ResultTests
  {
    private Fixture fixture;

    /// <summary>
    /// Test setup.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
      fixture = new Fixture();
    }

    [Test]
    public void TestSuccess()
    {
      var value = fixture.Create<int>();

      var result = Result.Success<int, string>(value);

      Assert.IsTrue(result.IsSuccess);
      Assert.AreEqual(value, result.Value);
      Assert.Null(result.Error);
    }

    [Test]
    public void TestFailure()
    {
      var error = fixture.Create<string>();
      var result = Result.Failure<int, string>(error);

      Assert.IsFalse(result.IsSuccess);
      Assert.AreEqual(error, result.Error);
      Assert.AreEqual((int)default, result.Value);
    }

    [Test]
    public void TestBindSuccess()
    {
      var value = fixture.Create<int>();

      IResult<int, string> functionOne() => Result.Success<int, string>(value);

      IResult<string, string> functionTwo(int input) => Result.Success<string, string>(input.ToString());

      IResult<int, string> functionThree(string input) => Result.Success<int, string>(input.Length);

      var result = functionOne()
        .Bind(r => functionTwo(r))
        .Bind(r => functionThree(r));

      Assert.IsTrue(result.IsSuccess);
      Assert.AreEqual(value.ToString().Length, result.Value);
      Assert.IsNull(result.Error);
    }

    [Test]
    public void TestBindFailure()
    {
      var value = fixture.Create<int>();
      var error = fixture.Create<string>();
      var functionThreeCalled = false;

      IResult<int, string> functionOne() => Result.Success<int, string>(value);

      IResult<string, string> functionTwo(int input) => Result.Failure<string, string>(error);

      IResult<int, string> functionThree(string input)
      {
        functionThreeCalled = true;
        return Result.Success<int, string>(input.Length);
      }

      var result = functionOne()
        .Bind(r => functionTwo(r))
        .Bind(r => functionThree(r));

      Assert.IsFalse(result.IsSuccess);
      Assert.AreEqual(error, result.Error);
      Assert.False(functionThreeCalled);
      Assert.AreEqual((int)default, result.Value);
    }

    [Test]
    public void TestMapSuccess()
    {
      var value = fixture.Create<int>();

      IResult<int, string> functionOne() => Result.Success<int, string>(value);

      string functionTwo(int input) => input.ToString();

      int functionThree(string input) => input.Length;

      var result = functionOne()
        .Map(r => functionTwo(r))
        .Map(r => functionThree(r));

      Assert.IsTrue(result.IsSuccess);
      Assert.AreEqual(value.ToString().Length, result.Value);
      Assert.IsNull(result.Error);
    }

    [Test]
    public void TestMapFailure()
    {
      var value = fixture.Create<int>();
      var error = fixture.Create<string>();
      var functionTwoCalled = false;
      var functionThreeCalled = false;

      IResult<int, string> functionOne() =>
        Result.Failure<int, string>(error);

      string functionTwo(int input)
      {
        functionTwoCalled = true;
        return input.ToString();
      }

      int functionThree(string input)
      {
        functionThreeCalled = true;
        return input.Length;
      }

      var result = functionOne()
        .Map(r => functionTwo(r))
        .Map(r => functionThree(r));

      Assert.IsFalse(result.IsSuccess);
      Assert.AreEqual(error, result.Error);
      Assert.False(functionTwoCalled);
      Assert.False(functionThreeCalled);
      Assert.AreEqual((int)default, result.Value);
    }
  }
}
