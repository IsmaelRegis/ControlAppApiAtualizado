using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ControlApp.Domain.Interfaces.Services
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile file);
    }
}
