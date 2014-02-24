using System.Collections.Specialized;

namespace Umbraco.Core.Security
{
    /// <summary>
    /// An interface for exposing the content type properties for storing membership data in when
    /// a membership provider's data is backed by an Umbraco content type. 
    /// </summary>
    public interface IUmbracoMemberTypeMembershipProvider
    {

        string LockPropertyTypeAlias { get;  }
        string LastLockedOutPropertyTypeAlias { get;  }
        string FailedPasswordAttemptsPropertyTypeAlias { get;  }
        string ApprovedPropertyTypeAlias { get;  }
        string CommentPropertyTypeAlias { get;  }
        string LastLoginPropertyTypeAlias { get;  }
        string LastPasswordChangedPropertyTypeAlias { get;  }
        string PasswordRetrievalQuestionPropertyTypeAlias { get;  }
        string PasswordRetrievalAnswerPropertyTypeAlias { get;  }

    }
}