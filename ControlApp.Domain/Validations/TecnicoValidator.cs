using System;
using System.Linq;
using FluentValidation;
using ControlApp.Domain.Entities;

namespace ControlApp.Domain.Validations
{
    public class TecnicoValidator : AbstractValidator<Tecnico>
    {
        public TecnicoValidator()
        {
            RuleFor(t => t.Cpf)
                .Matches(@"^\d{11}$").WithMessage("CPF deve conter 11 dígitos numéricos.")
                .NotEmpty().WithMessage("O CPF não pode ser vazio.")
                .Must(ValidarCpf).WithMessage("O CPF informado é inválido.");       


        }

        private bool ValidarCpf(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf) || cpf.Length != 11)
                return false;

            // Elimina CPFs inválidos conhecidos (ex: "00000000000")
            if (cpf.All(c => c == cpf[0]))
                return false;

            // Cálculo para verificar os dois últimos dígitos
            int[] multiplicadores1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicadores2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            // Cálculo do primeiro dígito verificador
            var soma1 = cpf.Take(9).Select((digit, index) => int.Parse(digit.ToString()) * multiplicadores1[index]).Sum();
            var resto1 = soma1 % 11;
            var digito1 = (resto1 < 2) ? 0 : 11 - resto1;

            // Cálculo do segundo dígito verificador
            var soma2 = cpf.Take(10).Select((digit, index) => int.Parse(digit.ToString()) * multiplicadores2[index]).Sum();
            var resto2 = soma2 % 11;
            var digito2 = (resto2 < 2) ? 0 : 11 - resto2;

            // Verifica se os dois dígitos calculados são iguais aos dígitos do CPF
            return cpf[9] == digito1.ToString()[0] && cpf[10] == digito2.ToString()[0];
        }
    }
}
