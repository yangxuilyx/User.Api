using System.Threading;
using System.Threading.Tasks;
using Contact.API.Dtos;

namespace Contact.API.Data
{
    public interface IContactRepository
    {
        Task<bool> UpdateContactInfo(BaseUserInfo userInfo, CancellationToken cancellationToken);

        Task<bool> AddContactAsync(int userId, BaseUserInfo userInfo, CancellationToken cancellationToken);
    }
}