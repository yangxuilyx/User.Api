using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contact.API.Dtos;

namespace Contact.API.Data
{
    public interface IContactRepository
    {
        Task<bool> UpdateContactInfo(BaseUserInfo userInfo, CancellationToken cancellationToken);

        Task<bool> AddContactAsync(int userId, BaseUserInfo userInfo, CancellationToken cancellationToken);

        Task<List<Models.Contact>> GetContactsAsync(int userId, CancellationToken cancellationToken);

        Task<bool> TagContactAsync(int userId, int contactId, List<string> tags, CancellationToken cancellationToken);
    }
}