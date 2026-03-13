using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CrustFlowERP.Services
{
    public interface IStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string folderName);
        Task DeleteFileAsync(string fileUrl);
    }
}
