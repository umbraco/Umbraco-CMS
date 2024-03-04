import { UmbDeleteFolderEntityAction } from './delete-folder.action.js';
import type { UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbBackofficeManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.Folder.Delete',
	matchKind: 'folderDelete',
	matchType: 'entityAction',
	manifest: {
		type: 'entityAction',
		kind: 'folderDelete',
		api: UmbDeleteFolderEntityAction,
		weight: 700,
		forEntityTypes: [],
		meta: {
			icon: 'icon-trash',
			label: 'Delete Folder...',
		},
	},
};
