using FluentValidation;
using TimePlanner.WebApi.Models.Requests;

namespace TimePlanner.WebApi.Validators
{
  public class WorkItemCreateRequestValidator : AbstractValidator<CreateWorkItemRequest>
  {
    public WorkItemCreateRequestValidator()
    {
      RuleFor(request => request.Name).NotEmpty();
    }
  }
}
