using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using ControlApp.Domain.Interfaces.Services;

namespace ControlApp.Domain.Services
{
    public class ImageService : IImageService
    {
        private readonly string _uploadDirectory;

        public ImageService()
        {
            // Obtendo o caminho absoluto da pasta wwwroot/images
            _uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

            if (!Directory.Exists(_uploadDirectory))
            {
                Directory.CreateDirectory(_uploadDirectory);
            }
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Arquivo inválido.");
            }

            // Gerando um nome único para a imagem
            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(_uploadDirectory, fileName);

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao salvar a imagem: {ex.Message}");
            }

            // Retorna o caminho relativo que pode ser usado no frontend
            return $"/images/{fileName}";
        }
    }
}