import type { ManifestPermission } from '@umbraco-cms/backoffice/extension-registry';

const permissions: Array<ManifestPermission> = [
	{
		type: 'permission',
		alias: 'Umb.Permission.BrowseDocument',
		name: 'Read Document Permission',
		meta: {
			label: 'Read',
      description: 'Allow access to browse documents',
		},
	},
  {
		type: 'permission',
		alias: 'Umb.Permission.CreateDocumentBlueprint',
		name: 'Create Document Blueprint Permission',
		meta: {
			label: 'Create Content Template',
      description: 'Allow access to create a Content Template'
		},
	},
  {
		type: 'permission',
		alias: 'Umb.Permission.DeleteDocument',
		name: 'Delete Document Permission',
		meta: {
			label: 'Delete',
      description: 'Allow access to create a Content Template'
		},
	},
];

export const manifests = [...permissions];
