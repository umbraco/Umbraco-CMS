using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Web.Models;

namespace Umbraco.Web.Hubs
{
    [HubName("NodeEditsHub")]
    public class NodeEditsHub : Hub
    {
        internal const string EditGroup = "NodeEditsGroup";
        private readonly static ConcurrentDictionary<string, EditInfo> _connections = new ConcurrentDictionary<string, EditInfo>();

        public class EditInfo
        {
            public int UserId { get; set; }
            public int NodeId { get; set; }
            public string UserName { get; set; }
        }

        public void Subscribe(int userId, string userName)
        {
            var user = new EditInfo
            {
                UserId = userId,
                UserName = userName
            };

            if (_connections.ContainsKey(Context.ConnectionId))
            {
                EditInfo oldUser;
                _connections.TryRemove(Context.ConnectionId, out oldUser);
            }

            //Save user and connection info for later use
            _connections.TryAdd(Context.ConnectionId, user);

            //Subscribe the user to our group
            Groups.Add(Context.ConnectionId, EditGroup);
        }

        public List<NodeEdit> ClaimSingleNode(NodeEdit edit)
        {
            EditInfo editInfo;
            if (_connections.TryGetValue(Context.ConnectionId, out editInfo))
            {
                editInfo.NodeId = edit.NodeId;
            }
            else
            {
                //Add the data, this can happen when a recycle happens
                var user = new EditInfo
                {
                    UserId = edit.UserId,
                    UserName = edit.UserName,
                    NodeId = edit.NodeId
                };

                _connections.TryAdd(Context.ConnectionId, user);
            }

            var allEdits = GetAll().ToList();
            //Notify all others with the new edits collection
            Clients.OthersInGroup(EditGroup).editsChanged(allEdits);

            return allEdits;
        }

        //public void ClaimNode(EditDTO edit)
        //{
        //    var context = new EditContext();
        //    edit.Date = DateTime.Now;
        //    context.Add(edit);

        //    var allEdits = context.GetAll();
        //    Clients.OthersInGroup(EditGroup).editsChanged(allEdits);
        //}

        //public void GreetAll(string userName, string message)
        //{
        //    var context = GlobalHost.ConnectionManager.GetHubContext<EditingHub>();
        //    context.Clients.Group(EditGroup).recieveMessage(userName, message);
        //}

        public override Task OnDisconnected(bool stopCalled)
        {
            EditInfo user;
            //Remove the old user's connection
            if (_connections.TryRemove(Context.ConnectionId, out user))
            {
                var allEdits = GetAll().ToList();
                //Notify all others with the new edits collection
                Clients.OthersInGroup(EditGroup).editsChanged(allEdits);

                ////Let all others know this user left
                //Clients.OthersInGroup(EditGroup).userDisconnected(user.UserId, user.UserName);
            }
            return base.OnDisconnected(stopCalled);
        }

        private IEnumerable<NodeEdit> GetAll()
        {
            return _connections.Select(e => new NodeEdit
            {
                ConnectionId = e.Key,
                NodeId = e.Value.NodeId,
                UserId = e.Value.UserId,
                UserName = e.Value.UserName
            });
        }

        public override Task OnReconnected()
        {
            ////Send all edits to the reconnected client
            //var allEdits = EditContext.GetAll();
            //Clients.Caller.editsChanged(allEdits);

            return base.OnReconnected();
        }
    }
}
