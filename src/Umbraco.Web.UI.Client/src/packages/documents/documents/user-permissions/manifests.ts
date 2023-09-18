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

const permissions: Array<ManifestUserPermission> = [
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_READ,
		name: 'Read Document User Permission',
		meta: {
			label: 'Read',
			description: 'Allow access to browse documents',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_CREATE_BLUEPRINT,
		name: 'Create Document Blueprint User Permission',
		meta: {
			label: 'Create Content Template',
			description: 'Allow access to create a Content Template',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_DELETE,
		name: 'Delete Document User Permission',
		meta: {
			label: 'Delete',
			description: 'Allow access to delete a document',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_CREATE,
		name: 'Create Document User Permission',
		meta: {
			label: 'Create',
			description: 'Allow access to create a document',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_NOTIFICATIONS,
		name: 'Document Notifications User Permission',
		meta: {
			label: 'Notifications',
			description: 'Allow access to setup notifications for documents',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_PUBLISH,
		name: 'Publish Document User Permission',
		meta: {
			label: 'Publish',
			description: 'Allow access to publish a document',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_PERMISSIONS,
		name: 'Document Permissions User Permission',
		meta: {
			label: 'Permissions',
			description: 'Allow access to change permissions for a document',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_SEND_FOR_APPROVAL,
		name: 'Send Document For Approval User Permission',
		meta: {
			label: 'Send For Approval',
			description: 'Allow access to send a document for approval before publishing',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_UNPUBLISH,
		name: 'Unpublish Document User Permission',
		meta: {
			label: 'Unpublish',
			description: 'Allow access to unpublish a document',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_UPDATE,
		name: 'Update Document User Permission',
		meta: {
			label: 'Update',
			description: 'Allow access to save a document',
		},
	},
];

export const granularPermissions: Array<ManifestUserGranularPermission> = [
	{
		type: 'userGranularPermission',
		alias: 'Umb.UserGranularPermission.Document',
		name: 'Document Granular User Permission',
		meta: {
			entityType: ['document'],
		},
	},
];

export const manifests = [...permissions, ...granularPermissions];
