import { UmbDeleteEntityAction } from './delete.action.js';
import type { UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbBackofficeManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.Delete',
	matchKind: 'delete',
	matchType: 'entityAction',
	manifest: {
		type: 'entityAction',
		kind: 'delete',
		api: UmbDeleteEntityAction,
		weight: 900,
		meta: {
			icon: 'icon-trash',
			label: 'Delete...',
		},
	},
};
