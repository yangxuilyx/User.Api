using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contact.API.Models;
using MongoDB.Driver;

namespace Contact.API.Data
{
    public class MongoContactApplyRequestRepository : IContactApplyRequestRepository
    {
        private readonly ContactContext _contactContext;

        public MongoContactApplyRequestRepository(ContactContext contactContext)
        {
            _contactContext = contactContext;
        }

        public async Task<bool> AddRequestAsync(ContactApplyRequest request, CancellationToken cancellationToken)
        {
            var filter = Builders<ContactApplyRequest>.Filter.Where(p => p.UserId == request.UserId && p.ApplierId == request.ApplierId);

            if ((await _contactContext.ContactApplyRequests.CountDocumentsAsync(filter,
                    cancellationToken: cancellationToken)) > 0)
            {
                var update = Builders<ContactApplyRequest>.Update.Set(r => r.ApplyTime, DateTime.Now);

                var updateResult = await _contactContext.ContactApplyRequests.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
                return updateResult.MatchedCount == updateResult.ModifiedCount;
            }

            await _contactContext.ContactApplyRequests.InsertOneAsync(request, null, cancellationToken);
            return true;
        }

        public async Task<bool> ApprovalAsync(int userId, int applierId, CancellationToken cancellationToken)
        {
            var filter = Builders<ContactApplyRequest>.Filter.Where(p => p.UserId == userId && p.ApplierId == applierId);

            if ((await _contactContext.ContactApplyRequests.CountDocumentsAsync(filter,
                    cancellationToken: cancellationToken)) > 0)
            {
                var update = Builders<ContactApplyRequest>.Update.Set(r => r.HandledTime, DateTime.Now)
                    .Set(p => p.Approvaled, 1);

                var updateResult = await _contactContext.ContactApplyRequests.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
                return updateResult.MatchedCount == updateResult.ModifiedCount;
            }

            return false;
        }

        public async Task<List<ContactApplyRequest>> GetRequestList(int userId, CancellationToken cancellationToken)
        {
            return (await _contactContext.ContactApplyRequests.FindAsync(p => p.UserId == userId)).ToList();
        }
    }
}