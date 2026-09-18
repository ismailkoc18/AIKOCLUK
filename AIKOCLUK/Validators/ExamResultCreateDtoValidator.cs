using AIKOCLUK.Models;
using FluentValidation;

namespace AIKOCLUK.Validators
{
    public class ExamResultCreateDtoValidator : AbstractValidator<ExamResultCreateDto>
    {
        public ExamResultCreateDtoValidator()
        {
            RuleFor(x => x.ExamType)
                .NotEmpty().WithMessage("Sınav türü boş bırakılamaz.")
                .Must(x => x == "TYT" || x == "AYT").WithMessage("Sınav türü yalnızca 'TYT' veya 'AYT' olabilir.");

            // YKS'de 4 yanlış 1 doğruyu götürdüğü için minimum -10, maksimum 40 net girilebilir
            RuleFor(x => x.TurkishNet)
                .InclusiveBetween(-10.0, 40.0).WithMessage("Türkçe neti -10 ile 40 arasında olmalıdır.");

            RuleFor(x => x.MathNet)
                .InclusiveBetween(-10.0, 40.0).WithMessage("Matematik neti -10 ile 40 arasında olmalıdır.");

            RuleFor(x => x.ScienceNet)
                .InclusiveBetween(-10.0, 40.0).WithMessage("Fen Bilimleri neti -10 ile 40 arasında olmalıdır.");

            RuleFor(x => x.SocialNet)
                .InclusiveBetween(-10.0, 40.0).WithMessage("Sosyal Bilimler neti -10 ile 40 arasında olmalıdır.");

            RuleFor(x => x.ExamDate)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Sınav tarihi gelecek bir tarih olamaz.");
        }
    }
}