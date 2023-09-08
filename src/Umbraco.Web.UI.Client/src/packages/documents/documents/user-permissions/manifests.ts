import type { ManifestUserPermission } from '@umbraco-cms/backoffice/extension-registry';

const permissions: Array<ManifestUserPermission> = [
	{
		type: 'userPermission',
		alias: 'Umb.UserPermission.Document.Browse',
		name: 'Read Document User Permission',
		meta: {
			label: 'Read',
			description: 'Allow access to browse documents',
		},
	},
	{
		type: 'userPermission',
		alias: 'Umb.UserPermission.Document.CreateBlueprint',
		name: 'Create Document Blueprint User Permission',
		meta: {
			label: 'Create Content Template',
			description: 'Allow access to create a Content Template',
		},
	},
	{
		type: 'userPermission',
		alias: 'Umb.UserPermission.Document.Delete',
		name: 'Delete Document User Permission',
		meta: {
			label: 'Delete',
			description: 'Allow access to delete a document',
		},
	},
	{
		type: 'userPermission',
		alias: 'Umb.UserPermission.Document.Create',
		name: 'Create Document User Permission',
		meta: {
			label: 'Create',
			description: 'Allow access to create a document',
		},
	},
	{
		type: 'userPermission',
		alias: 'Umb.UserPermission.Document.Notifications',
		name: 'Document Notifications User Permission',
		meta: {
			label: 'Notifications',
			description: 'Allow access to setup notifications for documents',
		},
	},
	{
		type: 'userPermission',
		alias: 'Umb.UserPermission.Document.Publish',
		name: 'Publish Document User Permission',
		meta: {
			label: 'Publish',
			description: 'Allow access to publish a document',
		},
	},
	{
		type: 'userPermission',
		alias: 'Umb.UserPermission.Document.Permissions',
		name: 'Document Permissions User Permission',
		meta: {
			label: 'Permissions',
			description: 'Allow access to change permissions for a document',
		},
	},
	{
		type: 'userPermission',
		alias: 'Umb.UserPermission.Document.SendForApproval',
		name: 'Send Document For Approval User Permission',
		meta: {
			label: 'Send For Approval',
			description: 'Allow access to send a document for approval before publishing',
		},
	},
	{
		type: 'userPermission',
		alias: 'Umb.UserPermission.Document.Unpublish',
		name: 'Unpublish Document User Permission',
		meta: {
			label: 'Unpublish',
			description: 'Allow access to unpublish a document',
		},
	},
	{
		type: 'userPermission',
		alias: 'Umb.UserPermission.Document.Update',
		name: 'Update Document User Permission',
		meta: {
			label: 'Update',
			description: 'Allow access to save a document',
		},
	},
];

export const manifests = [...permissions];
