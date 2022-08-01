using System.Collections.Generic;
using task5.Contexts;

namespace task5.Managers
{
    public interface IUserManager
    {
        public IReadOnlyList<string> GetConnectionsUser(string user);

        public void Start(MessengerContext dbContext);

        public bool IsUserCreated(MessengerContext dbContext, string name);

        public bool AddUser(MessengerContext dbContext, string name);

        public bool AddConnection(MessengerContext dbContext, string name, string id);

        public void RemoveConnection(string id);
    }
}
