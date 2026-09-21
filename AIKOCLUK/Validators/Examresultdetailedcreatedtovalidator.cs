using AIKOCLUK.Models;
using FluentValidation;

namespace AIKOCLUK.Validators
{
    public class ExamResultDetailedCreateDtoValidator : AbstractValidator<ExamResultDetailedCreateDto>
    {
        public ExamResultDetailedCreateDtoValidator()
        {
            // ExamResultCreateDtoValidator içindeki temel alan kurallarını (ExamType, netler, tarih) devralır
            Include(new ExamResultCreateDtoValidator());

            RuleFor(x => x.TopicErrors)
                .NotNull()
                .Must(list => list.Count <= 50).WithMessage("Bir denemede en fazla 50 konu hatası girilebilir.");

            RuleForEach(x => x.TopicErrors).SetValidator(new TopicErrorDtoValidator());
        }
    }

    public class TopicErrorDtoValidator : AbstractValidator<TopicErrorDto>
    {
        public TopicErrorDtoValidator()
        {
            RuleFor(x => x.Subject)
                .NotEmpty().WithMessage("Ders alanı boş bırakılamaz.")
                .MaximumLength(100);

            RuleFor(x => x.TopicName)
                .NotEmpty().WithMessage("Konu adı boş bırakılamaz.")
                .MaximumLength(100);

            RuleFor(x => x.IncorrectCount)
                .GreaterThanOrEqualTo(0).WithMessage("Yanlış sayısı negatif olamaz.");

            RuleFor(x => x.BlankCount)
                .GreaterThanOrEqualTo(0).WithMessage("Boş sayısı negatif olamaz.");
        }
    }
}