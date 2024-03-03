import { UmbRenameEntityAction } from './rename.action.js';
import type { UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbBackofficeManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.Rename',
	matchKind: 'rename',
	matchType: 'entityAction',
	manifest: {
		type: 'entityAction',
		kind: 'rename',
		api: UmbRenameEntityAction,
		weight: 200,
		forEntityTypes: [],
		meta: {
			icon: 'icon-edit',
			label: 'Rename...',
			renameRepositoryAlias: '',
			itemRepositoryAlias: '',
		},
	},
};
