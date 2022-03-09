using FluentValidation;
using TimePlanner.WebApi.Models.Requests;

namespace TimePlanner.WebApi.Validators
{
  public class WorkItemRequestValidator : AbstractValidator<WorkItemRequest>
  {
    public WorkItemRequestValidator()
    {
      RuleFor(request => request.Name).NotEmpty();
    }
  }
}
