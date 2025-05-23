﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlApp.Domain.Entities
{
    public class Endereco
    {
        public Guid EnderecoId { get; set; }
        public string? Cep { get; set; }
        public string? Logradouro { get; set; }
        public string? Bairro { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
        public string? Complemento { get; set; }
        public string? Numero { get; set; }
    }
}
