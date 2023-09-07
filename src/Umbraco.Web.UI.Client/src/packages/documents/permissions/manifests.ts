import type { ManifestPermission } from '@umbraco-cms/backoffice/extension-registry';

const permissions: Array<ManifestPermission> = [
	{
		type: 'permission',
		alias: 'Umb.Permission.Document.Browse',
		name: 'Read Document Permission',
		meta: {
			label: 'Read',
      description: 'Allow access to browse documents',
		},
	},
  {
		type: 'permission',
		alias: 'Umb.Permission.Document.CreateBlueprint',
		name: 'Create Document Blueprint Permission',
		meta: {
			label: 'Create Content Template',
      description: 'Allow access to create a Content Template'
		},
	},
  {
		type: 'permission',
		alias: 'Umb.Permission.Document.Delete',
		name: 'Delete Document Permission',
		meta: {
			label: 'Delete',
      description: 'Allow access to delete a document'
		},
	},
	{
		type: 'permission',
		alias: 'Umb.Permission.Document.Create',
		name: 'Create Document Permission',
		meta: {
			label: 'Create',
      description: 'Allow access to create a document'
		},
	},
	{
		type: 'permission',
		alias: 'Umb.Permission.Document.Notifications',
		name: 'Document Notifications Permission',
		meta: {
			label: 'Notifications',
      description: 'Allow access to setup notifications for documents'
		},
	},
	{
		type: 'permission',
		alias: 'Umb.Permission.Document.Publish',
		name: 'Publish Document Permission',
		meta: {
			label: 'Publish',
      description: 'Allow access to publish a document'
		},
	},
	{
		type: 'permission',
		alias: 'Umb.Permission.Document.Permissions',
		name: 'Document Permissions Permission',
		meta: {
			label: 'Permissions',
      description: 'Allow access to change permissions for a document'
		},
	},
	{
		type: 'permission',
		alias: 'Umb.Permission.Document.SendForApproval',
		name: 'Send Document For Approval Permission',
		meta: {
			label: 'Send For Approval',
      description: 'Allow access to send a document for approval before publishing'
		},
	},
	{
		type: 'permission',
		alias: 'Umb.Permission.Document.Unpublish',
		name: 'Unpublish Document Permission',
		meta: {
			label: 'Unpublish',
      description: 'Allow access to unpublish a document'
		},
	},
	{
		type: 'permission',
		alias: 'Umb.Permission.Document.Update',
		name: 'Update Document Permission',
		meta: {
			label: 'Update',
      description: 'Allow access to save a document'
		},
	},
];

export const manifests = [...permissions];
