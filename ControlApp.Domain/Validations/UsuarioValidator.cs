using ControlApp.Domain.Entities;
using FluentValidation;

namespace ControlApp.Domain.Validations
{
    public class UsuarioValidator : AbstractValidator<Usuario>
    {
        public UsuarioValidator()
        {
            RuleFor(u => u.Nome)
                .NotEmpty().WithMessage("O nome do usuário não pode ser vazio.")
                .MaximumLength(100).WithMessage("O nome do usuário deve ter no máximo 100 caracteres.");

            RuleFor(u => u.Email)
                .NotEmpty().WithMessage("O email não pode ser vazio.")
                .EmailAddress().WithMessage("O email deve ter um formato válido.")
                .MaximumLength(150).WithMessage("O email não pode ter mais de 150 caracteres.");

            RuleFor(u => u.Senha)
                .NotEmpty().WithMessage("A senha não pode ser vazia.")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")
                    .WithMessage("A senha deve ter letras maiúsculas, minúsculas, números, símbolos e pelo menos 8 caracteres.");

            RuleFor(u => u.Role)
                .IsInEnum().WithMessage("O papel (Role) do usuário deve ser válido.");

        }
    }
}