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
                throw new ArgumentException("Arquivo inválido."); // Lança erro se o arquivo estiver vazio ou nulo
            }

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName); // Cria um nome único pra imagem com a extensão original
            var filePath = Path.Combine(_uploadDirectory, fileName); // Monta o caminho completo pra salvar a imagem

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream); // Salva a imagem no caminho definido
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao salvar a imagem: {ex.Message}"); // Lança erro se algo der errado no salvamento
            }
            return $"/images/{fileName}"; // Retorna o caminho pra acessar a imagem
        }
        #endregion
    }
}