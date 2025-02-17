using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlApp.Domain.Entities;
using FluentValidation;

namespace ControlApp.Domain.Validations
{
    public class TrajetoValidator : AbstractValidator<Trajeto>
    {
        public TrajetoValidator()
        {
            RuleFor(t => t.Data)
                .NotEmpty().WithMessage("A data do trajeto deve ser informada.");

            RuleFor(t => t.UsuarioId)
                .NotEmpty().WithMessage("O técnico responsável pelo trajeto deve ser informado.");

            RuleForEach(t => t.Localizacoes)
                .ChildRules(loc =>
                {
                    loc.RuleFor(l => l.Latitude)
                        .NotEmpty().WithMessage("A latitude da localização deve ser informada.");
                    loc.RuleFor(l => l.Longitude)
                        .NotEmpty().WithMessage("A longitude da localização deve ser informada.");
                });
        }
    }
}
