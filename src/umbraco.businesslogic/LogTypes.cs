using System;

namespace umbraco.BusinessLogic
{
	/// <summary>
	/// The collection of available log types.
	/// </summary>
	public enum LogTypes
	{
		/// <summary>
		/// Used when new nodes are added
		/// </summary>
		[AuditTrailLogItem]
		New,
		/// <summary>
		/// Used when nodes are saved
		/// </summary>
		[AuditTrailLogItem]
		Save,
		/// <summary>
		/// Used when nodes are opened
		/// </summary>
		[AuditTrailLogItem]
		Open,
		/// <summary>
		/// Used when nodes are deleted
		/// </summary>
		[AuditTrailLogItem]
		Delete,
		/// <summary>
		/// Used when nodes are published
		/// </summary>
		[AuditTrailLogItem]
		Publish,
		/// <summary>
		/// Used when nodes are send to publishing
		/// </summary>
		[AuditTrailLogItem]
		SendToPublish,
		/// <summary>
		/// Used when nodes are unpublished
		/// </summary>
		[AuditTrailLogItem]
		UnPublish,
		/// <summary>
		/// Used when nodes are moved
		/// </summary>
		[AuditTrailLogItem]
		Move,
		/// <summary>
		/// Used when nodes are copied
		/// </summary>
		[AuditTrailLogItem]
		Copy,
		/// <summary>
		/// Used when nodes are assígned a domain
		/// </summary>
		[AuditTrailLogItem]
		AssignDomain,
		/// <summary>
		/// Used when public access are changed for a node
		/// </summary>
		[AuditTrailLogItem]
		PublicAccess,
		/// <summary>
		/// Used when nodes are sorted
		/// </summary>
		[AuditTrailLogItem]
		Sort,
		/// <summary>
		/// Used when a notification are send to a user
		/// </summary>
		[AuditTrailLogItem]
		Notify,

		/// <summary>
		/// Used when a user logs into the umbraco back-end
		/// </summary>
		[Obsolete("Use LogHelper to write log messages")]
		Login,

		/// <summary>
		/// Used when a user logs out of the umbraco back-end
		/// </summary>
		[Obsolete("Use LogHelper to write log messages")]
		Logout,
		
		/// <summary>
		/// Used when a user login fails
		/// </summary>
		[Obsolete("Use LogHelper to write error log messages")]
		LoginFailure,
		
		/// <summary>
		/// General system notification
		/// </summary>
		[AuditTrailLogItem]
		System,


		/// <summary>
		/// System debugging notification
		/// </summary>
		[Obsolete("Use LogHelper to write debug log messages")]
		Debug,

		/// <summary>
		/// System error notification
		/// </summary>
		[Obsolete("Use LogHelper to write error log messages")]
		Error,

		/// <summary>
		/// Notfound error notification
		/// </summary>
		[Obsolete("Use LogHelper to write log messages")]
		NotFound,

		/// <summary>
		/// Used when a node's content is rolled back to a previous version
		/// </summary>
		[AuditTrailLogItem]
		RollBack,
		/// <summary>
		/// Used when a package is installed
		/// </summary>
		[AuditTrailLogItem]
		PackagerInstall,
		/// <summary>
		/// Used when a package is uninstalled
		/// </summary>
		[AuditTrailLogItem]
		PackagerUninstall,

		/// <summary>
		/// Used when a ping is send to/from the system
		/// </summary>
		[Obsolete("Use LogHelper to write log messages")]
		Ping,
		/// <summary>
		/// Used when a node is send to translation
		/// </summary>
		[AuditTrailLogItem]
		SendToTranslate,
		
		/// <summary>
		/// Notification from a Scheduled task.
		/// </summary>
		[Obsolete("Use LogHelper to write log messages")]
		ScheduledTask,

		/// <summary>
		/// Use this log action for custom log messages that should be shown in the audit trail
		/// </summary>
		[AuditTrailLogItem]
		[Obsolete("Use LogHelper to write custom log messages")]
		Custom
	}
}