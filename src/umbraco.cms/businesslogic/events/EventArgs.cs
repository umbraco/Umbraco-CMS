using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Core;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.web;

namespace umbraco.cms.businesslogic {
    //Content Event args
    public class PublishEventArgs : System.ComponentModel.CancelEventArgs { }
    public class MoveEventArgs : System.ComponentModel.CancelEventArgs { }
    
    
    public class UnPublishEventArgs : System.ComponentModel.CancelEventArgs { }
    
    
    public class MoveToTrashEventArgs : System.ComponentModel.CancelEventArgs { }

    //Content Cache Event args
    public class DocumentCacheEventArgs : System.ComponentModel.CancelEventArgs { }
    public class RefreshContentEventArgs : System.ComponentModel.CancelEventArgs { }

    //Generel eventArgs
    public class DeleteEventArgs : System.ComponentModel.CancelEventArgs { }
    public class SaveEventArgs : System.ComponentModel.CancelEventArgs { }
    public class NewEventArgs : System.ComponentModel.CancelEventArgs { }
    
    //Special Members Event args
    public class AddToCacheEventArgs : GroupEventArgs { }
    public class RemoveFromCacheEventArgs : GroupEventArgs { }
    public class AddGroupEventArgs : GroupEventArgs { }
    public class RemoveGroupEventArgs : GroupEventArgs { }
    public class GroupEventArgs : System.ComponentModel.CancelEventArgs
    {
        public int GroupId{ get; set; }
    }
    
    //Access event args
    public class AddProtectionEventArgs : System.ComponentModel.CancelEventArgs { }
    public class RemoveProtectionEventArgs : System.ComponentModel.CancelEventArgs { }
    public class AddMemberShipRoleToDocumentEventArgs : System.ComponentModel.CancelEventArgs { }
    public class RemoveMemberShipRoleFromDocumentEventArgs : System.ComponentModel.CancelEventArgs { }
    public class RemoveMemberShipUserFromDocumentEventArgs : System.ComponentModel.CancelEventArgs { }
    public class AddMembershipUserToDocumentEventArgs : System.ComponentModel.CancelEventArgs { }
    

    public class ContentCacheLoadNodeEventArgs : System.ComponentModel.CancelEventArgs
    {
        public bool CancelChildren { get; set; }
    }

    
}
