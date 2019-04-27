using System.Collections.Generic;
using Contact.API.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Contact.API.Data
{
    public class ContactContext
    {
        private readonly IMongoDatabase _mongoDatabase;
        private IMongoCollection<ContactBook> _collection;

        private readonly AppSettings _appSettings;

        public ContactContext(IOptionsSnapshot<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;

            var client = new MongoClient(_appSettings.MongoContactConnectionString);

            _mongoDatabase = client.GetDatabase(_appSettings.MongoContactDatabase);
        }

        private void CheckAndCreateCollection(string collectionName)
        {
            var collectionList = _mongoDatabase.ListCollections().ToList();
            var collectionNames = new List<string>();

            collectionList.ForEach(b => collectionNames.Add(b["name"].ToString()));

            if (!collectionNames.Contains(collectionName))
            {
                _mongoDatabase.CreateCollection(collectionName);
            }
        }

        /// <summary>
        /// 通讯录
        /// </summary>
        public IMongoCollection<ContactBook> ContactBooks
        {
            get
            {
                CheckAndCreateCollection("ContactBooks");
                return _mongoDatabase.GetCollection<ContactBook>("ContactBooks");
            }
        }

        /// <summary>
        /// 好友请求
        /// </summary>
        public IMongoCollection<ContactApplyRequest> ContactApplyRequests
        {
            get
            {
                CheckAndCreateCollection("ContactApplyRequest");
                return _mongoDatabase.GetCollection<ContactApplyRequest>("ContactApplyRequests");
            }
        }
    }
}