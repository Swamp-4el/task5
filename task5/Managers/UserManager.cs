using System.Collections.Generic;
using System.Linq;
using task5.Contexts;

namespace task5.Managers
{
    public class UserManager : IUserManager
    {
        private readonly Dictionary<string, List<string>> _users;
        private bool _isStart = true;

        public UserManager()
        {
            _users = new Dictionary<string, List<string>>();
        }

        public void Start(MessengerContext dbContext)
        {
            if (_isStart)
            {
                foreach (var user in dbContext.Users.ToList())
                    _users.Add(user.Id, new List<string>());
                _isStart = false;
            }
        }

        public IReadOnlyList<string> GetConnectionsUser(string name)
        {
            if (!_users.ContainsKey(name))
                return new List<string>();

            return _users[name];
        }

        public bool AddUser(MessengerContext dbContext, string name)
        {
            if (!IsUserCreated(dbContext, name))
                return false;

            _users.Add(name, new List<string>());

            return true;
        }

        public bool AddConnection(MessengerContext dbContext, string name, string id)
        {
            if (!IsUserCreated(dbContext, name) || string.IsNullOrWhiteSpace(id))
                return false;

            if (_users.ContainsKey(name))
                _users[name].Add(id);
            else
                _users.Add(name, new List<string> { id });

            return true;
        }

        public void RemoveConnection(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return;

            foreach (var user in _users)
            {
                if (user.Value.Remove(id))
                    return;
            }
        }

        public bool IsUserCreated(MessengerContext dbContext, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            var user = dbContext.Users.FirstOrDefault(u => u.Id == name);

            return !(user is null);
        }
    }
}
