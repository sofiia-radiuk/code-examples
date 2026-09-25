using System;
using System.Collections.Generic;

namespace GoogleDriveClient
{
    public class MemoryCredentialStore : CredentialStore
    {
        private readonly Dictionary<string, UserCredential> _store = new();

        public MemoryCredentialStore() : base("In-Memory Store")
        {
        }

        public int StoredCount => _store.Count;

        public override void SaveCredentials(string userId, UserCredential credential)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID не може бути порожнім.", nameof(userId));
            if (credential == null)
                throw new ArgumentNullException(nameof(credential));

            _store[userId.Trim()] = credential;
        }

        public override UserCredential? LoadCredentials(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            _store.TryGetValue(userId.Trim(), out var cred);
            return cred;
        }

        public override bool ClearCredentials(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return false;

            return _store.Remove(userId.Trim());
        }
    }
}