using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace ControlApp.Domain.Validations
{
    public class PontoValidator : AbstractValidator<Ponto>
    {
        public PontoValidator()
        {
            RuleFor(p => p.InicioExpediente)
                .NotEmpty().WithMessage("O início do expediente deve ser informado.");

            RuleFor(p => p.FimExpediente)
                .GreaterThan(p => p.InicioExpediente).WithMessage("O fim do expediente não pode ser anterior ao início.");

            RuleFor(p => p.InicioPausa)
                .NotEmpty().WithMessage("O início da pausa deve ser informado.");

            RuleFor(p => p.RetornoPausa)
                .GreaterThan(p => p.InicioPausa).WithMessage("O retorno da pausa não pode ser anterior ao início da pausa.");

            RuleFor(p => p.HorasTrabalhadas)
             .GreaterThan(TimeSpan.Zero).WithMessage("As horas trabalhadas devem ser um valor positivo.");

            RuleFor(p => p.HorasExtras)
                .GreaterThanOrEqualTo(TimeSpan.Zero).WithMessage("As horas extras não podem ser negativas.");

            RuleFor(p => p.HorasDevidas)
                .GreaterThanOrEqualTo(TimeSpan.Zero).WithMessage("As horas devidas não podem ser negativas.");
        }
    }
}
