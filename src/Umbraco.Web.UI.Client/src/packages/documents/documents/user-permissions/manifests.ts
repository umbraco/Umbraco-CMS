import type {
	ManifestUserGranularPermission,
	ManifestUserPermission,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_USER_PERMISSION_DOCUMENT_CREATE = 'Umb.UserPermission.Document.Create';
export const UMB_USER_PERMISSION_DOCUMENT_READ = 'Umb.UserPermission.Document.Read';
export const UMB_USER_PERMISSION_DOCUMENT_UPDATE = 'Umb.UserPermission.Document.Update';
export const UMB_USER_PERMISSION_DOCUMENT_DELETE = 'Umb.UserPermission.Document.Delete';
export const UMB_USER_PERMISSION_DOCUMENT_CREATE_BLUEPRINT = 'Umb.UserPermission.Document.CreateBlueprint';
export const UMB_USER_PERMISSION_DOCUMENT_NOTIFICATIONS = 'Umb.UserPermission.Document.Notifications';
export const UMB_USER_PERMISSION_DOCUMENT_PUBLISH = 'Umb.UserPermission.Document.Publish';
export const UMB_USER_PERMISSION_DOCUMENT_PERMISSIONS = 'Umb.UserPermission.Document.Permissions';
export const UMB_USER_PERMISSION_DOCUMENT_SEND_FOR_APPROVAL = 'Umb.UserPermission.Document.SendForApproval';
export const UMB_USER_PERMISSION_DOCUMENT_UNPUBLISH = 'Umb.UserPermission.Document.Unpublish';
export const UMB_USER_PERMISSION_DOCUMENT_COPY = 'Umb.UserPermission.Document.Copy';
export const UMB_USER_PERMISSION_DOCUMENT_MOVE = 'Umb.UserPermission.Document.Move';
export const UMB_USER_PERMISSION_DOCUMENT_SORT = 'Umb.UserPermission.Document.Sort';
export const UMB_USER_PERMISSION_DOCUMENT_CULTURE_AND_HOSTNAMES = 'Umb.UserPermission.Document.CultureAndHostnames';
export const UMB_USER_PERMISSION_DOCUMENT_PUBLIC_ACCESS = 'Umb.UserPermission.Document.PublicAccess';
export const UMB_USER_PERMISSION_DOCUMENT_ROLLBACK = 'Umb.UserPermission.Document.Rollback';

const permissions: Array<ManifestUserPermission> = [
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_READ,
		name: 'Read Document User Permission',
		meta: {
			entityType: 'document',
			label: 'Read',
			description: 'Allow access to browse documents',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_CREATE_BLUEPRINT,
		name: 'Create Document Blueprint User Permission',
		meta: {
			entityType: 'document',
			label: 'Create Content Template',
			description: 'Allow access to create a Content Template',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_DELETE,
		name: 'Delete Document User Permission',
		meta: {
			entityType: 'document',
			label: 'Delete',
			description: 'Allow access to delete a document',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_CREATE,
		name: 'Create Document User Permission',
		meta: {
			entityType: 'document',
			label: 'Create',
			description: 'Allow access to create a document',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_NOTIFICATIONS,
		name: 'Document Notifications User Permission',
		meta: {
			entityType: 'document',
			label: 'Notifications',
			description: 'Allow access to setup notifications for documents',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_PUBLISH,
		name: 'Publish Document User Permission',
		meta: {
			entityType: 'document',
			label: 'Publish',
			description: 'Allow access to publish a document',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_PERMISSIONS,
		name: 'Document Permissions User Permission',
		meta: {
			entityType: 'document',
			label: 'Permissions',
			description: 'Allow access to change permissions for a document',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_SEND_FOR_APPROVAL,
		name: 'Send Document For Approval User Permission',
		meta: {
			entityType: 'document',
			label: 'Send For Approval',
			description: 'Allow access to send a document for approval before publishing',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_UNPUBLISH,
		name: 'Unpublish Document User Permission',
		meta: {
			entityType: 'document',
			label: 'Unpublish',
			description: 'Allow access to unpublish a document',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_UPDATE,
		name: 'Update Document User Permission',
		meta: {
			entityType: 'document',
			label: 'Update',
			description: 'Allow access to save a document',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_COPY,
		name: 'Copy Document User Permission',
		meta: {
			entityType: 'document',
			label: 'Copy',
			description: 'Allow access to copy a document',
			group: 'structure',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_MOVE,
		name: 'Move Document User Permission',
		meta: {
			entityType: 'document',
			label: 'Move',
			description: 'Allow access to move a document',
			group: 'structure',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_SORT,
		name: 'Sort Document User Permission',
		meta: {
			entityType: 'document',
			label: 'Sort',
			description: 'Allow access to sort documents',
			group: 'structure',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_CULTURE_AND_HOSTNAMES,
		name: 'Document Culture And Hostnames User Permission',
		meta: {
			entityType: 'document',
			label: 'Culture And Hostnames',
			description: 'Allow access to set culture and hostnames for documents',
			group: 'administration',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_PUBLIC_ACCESS,
		name: 'Document Public Access User Permission',
		meta: {
			entityType: 'document',
			label: 'Public Access',
			description: 'Allow access to set and change access restrictions for a document',
			group: 'administration',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_ROLLBACK,
		name: 'Document Rollback User Permission',
		meta: {
			entityType: 'document',
			label: 'Rollback',
			description: 'Allow access to roll back a document to a previous state',
			group: 'administration',
		},
	},
];

export const granularPermissions: Array<ManifestUserGranularPermission> = [
	{
		type: 'userGranularPermission',
		alias: 'Umb.UserGranularPermission.Document',
		name: 'Document Granular User Permission',
		loader: () =>
			import('../components/input-document-granular-permission/input-document-granular-permission.element.js'),
		meta: {
			entityType: 'document',
		},
	},
];

export const manifests = [...permissions, ...granularPermissions];
