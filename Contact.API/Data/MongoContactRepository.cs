using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contact.API.Dtos;
using Contact.API.Models;
using Microsoft.EntityFrameworkCore.Internal;
using MongoDB.Driver;

namespace Contact.API.Data
{
    public class MongoContactRepository : IContactRepository
    {
        private readonly ContactContext _contactContext;

        public MongoContactRepository(ContactContext contactContext)
        {
            _contactContext = contactContext;
        }

        public async Task<bool> UpdateContactInfo(BaseUserInfo userInfo, CancellationToken cancellationToken)
        {
            var contactBook =
                (await _contactContext.ContactBooks.FindAsync(p => p.UserId == userInfo.UserId,
                    cancellationToken: cancellationToken)).FirstOrDefault(cancellationToken: cancellationToken);

            if (contactBook == null)
            {
                return true;
            }

            var contactIds = contactBook.Contacts.Select(p => p.UserId);

            var filter = Builders<ContactBook>.Filter.And(
                Builders<ContactBook>.Filter.In(c => c.UserId, contactIds),
                Builders<ContactBook>.Filter.ElemMatch(p => p.Contacts, c => c.UserId == userInfo.UserId));

            var update = Builders<ContactBook>.Update
                .Set("Contacts.$.Name", userInfo.Name)
                .Set("Contacts.$.Company", userInfo.Company)
                .Set("Contacts.$.Avatar", userInfo.Avatar)
                .Set("Contacts.$.PhoneNumber", userInfo.PhoneNumber)
                .Set("Contacts.$.Title", userInfo.Title);

            var updateResult = await _contactContext.ContactBooks.UpdateManyAsync(filter, update, cancellationToken: cancellationToken);

            return updateResult.MatchedCount == updateResult.ModifiedCount;
        }

        public async Task<bool> AddContactAsync(int userId, BaseUserInfo userInfo, CancellationToken cancellationToken)
        {
            if (_contactContext.ContactBooks.CountDocuments(p => p.UserId == userId) == 0)
            {
                await _contactContext.ContactBooks.InsertOneAsync(new ContactBook { UserId = userId }, null, cancellationToken);
            }

            var filter = Builders<ContactBook>.Filter.Eq(c => c.UserId, userId);

            var update = Builders<ContactBook>.Update.AddToSet(p => p.Contacts, new Models.Contact()
            {
                UserId = userInfo.UserId,
                Avatar = userInfo.Avatar,
                Company = userInfo.Company,
                Name = userInfo.Name,
                PhoneNumber = userInfo.PhoneNumber,
                Tags = new List<string>(),
                Title = userInfo.Title
            });

            var updateResult = await _contactContext.ContactBooks.UpdateOneAsync(filter, update, null, cancellationToken);

            return updateResult.MatchedCount == 1 && updateResult.MatchedCount == updateResult.ModifiedCount;
        }

        public async Task<List<Models.Contact>> GetContactsAsync(int userId, CancellationToken cancellationToken)
        {
            var contactBook = (await _contactContext.ContactBooks.FindAsync(p => p.UserId == userId, cancellationToken: cancellationToken)).FirstOrDefault();
            return contactBook?.Contacts;
        }

        public async Task<bool> TagContactAsync(int userId, int contactId, List<string> tags, CancellationToken cancellationToken)
        {
            var filter = Builders<ContactBook>.Filter.And(Builders<ContactBook>.Filter.Eq(p => p.UserId, userId),
                //Builders<ContactBook>.Filter.Eq(Contacts.UserId,contactId));
                Builders<ContactBook>.Filter.ElemMatch(p => p.Contacts, p => p.UserId == contactId));

            var update = Builders<ContactBook>.Update.Set("Contacts.$.Tags", tags);

            var updateResult = await _contactContext.ContactBooks.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
            return updateResult.MatchedCount == updateResult.ModifiedCount;
        }
    }
}