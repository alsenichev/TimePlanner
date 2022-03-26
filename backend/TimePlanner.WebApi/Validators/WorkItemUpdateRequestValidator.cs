using FluentValidation;
using TimePlanner.Domain.Models;
using TimePlanner.WebApi.Models.Requests;

namespace TimePlanner.WebApi.Validators
{
  public class WorkItemUpdateRequestValidator : AbstractValidator<UpdateWorkItemRequest>
  {
    public WorkItemUpdateRequestValidator()
    {
      RuleForEach(r => r.durations)
        .Must(d => TimeSpan.TryParse(d.value, out var _))
        .WithMessage("Duration value can not be converted to TimeSpan.")
        .Must(d => DateOnly.TryParse(d.date, out var _))
        .WithMessage("Duration date can not be converted to DateOnly.");
      When(r => r.wakingUp.HasValue, () =>
      {
        RuleFor(r => r.wakingUp.Value.when)
          .NotEmpty()
          .Must(w => DateOnly.TryParse(w, out var _))
          .WithMessage($"Waking up 'when' must be possible to convert to DateOnly");
        RuleFor(r => r.wakingUp.Value.where)
          .NotEmpty()
          .Must(w => Enum.TryParse<Category>(w, out var _))
          .WithMessage("Waking up 'where' must be possible to convert to Category enum.");
      });
    }
  }
}
