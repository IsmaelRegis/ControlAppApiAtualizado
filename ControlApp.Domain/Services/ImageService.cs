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

        #region Construtor
        public ImageService()
        {
            _uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images"); // Define o caminho onde as imagens vão ficar

            if (!Directory.Exists(_uploadDirectory))
            {
                Directory.CreateDirectory(_uploadDirectory); // Cria o diretório se ele não existir
            }
        }
        #endregion

        #region Método de Upload
        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Arquivo inválido.");
            }

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(_uploadDirectory, fileName);

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                Console.WriteLine($"Imagem salva em: {filePath}"); // Adicione este log
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao salvar a imagem: {ex.Message}");
            }
            return $"/images/{fileName}";
        }
        #endregion
    }
}