using FluentValidation;
using TimePlanner.Domain.Models;
using TimePlanner.WebApi.Models.Requests;

namespace TimePlanner.WebApi.Validators
{
  public class WorkItemUpdateRequestValidator : AbstractValidator<UpdateWorkItemRequest>
  {
    public WorkItemUpdateRequestValidator()
    {
      //RuleForEach(r => r.durations)
      //  .Must(d => TimeSpan.TryParse(d.value, out var _))
      //  .WithMessage("Duration value can not be converted to TimeSpan.")
      //  .Must(d => DateOnly.TryParse(d.date, out var _))
      //  .WithMessage("Duration date can not be converted to DateOnly.");
      RuleFor(r => r.Name).NotEmpty();
      RuleFor(r => r.Category).Must(c => Enum.TryParse(typeof(Category), c, true, out var _))
        .WithMessage("Can not parse the Category enum.");
    }
  }
}
