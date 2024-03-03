import { UmbUpdateFolderEntityAction } from './update-folder.action.js';
import type { UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbBackofficeManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.Folder.Update',
	matchKind: 'folderUpdate',
	matchType: 'entityAction',
	manifest: {
		type: 'entityAction',
		kind: 'folderUpdate',
		api: UmbUpdateFolderEntityAction,
		weight: 700,
		forEntityTypes: [],
		meta: {
			icon: 'icon-edit',
			label: 'Rename Folder...',
		},
	},
};
