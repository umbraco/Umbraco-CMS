import { UmbTrashEntityAction } from './trash.action.js';
import type { UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbBackofficeManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.Trash',
	matchKind: 'trash',
	matchType: 'entityAction',
	manifest: {
		type: 'entityAction',
		kind: 'trash',
		api: UmbTrashEntityAction,
		weight: 900,
		forEntityTypes: [],
		meta: {
			icon: 'icon-trash',
			label: 'Trash',
			itemRepositoryAlias: '',
			trashRepositoryAlias: '',
		},
	},
};
