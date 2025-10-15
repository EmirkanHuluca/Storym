using FluentValidation;
using Storym.Application.Diary.Commands;

namespace Storym.Application.Diary.Validators;

public sealed class CreateDiaryEntryValidator : AbstractValidator<CreateDiaryEntryCommand>
{
    public CreateDiaryEntryValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Summary).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Content).NotEmpty();
    }
}
