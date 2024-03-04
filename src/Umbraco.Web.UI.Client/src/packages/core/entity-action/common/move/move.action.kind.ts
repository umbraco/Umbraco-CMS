import { UmbMoveEntityAction } from './move.action.js';
import type { UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbBackofficeManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.Move',
	matchKind: 'move',
	matchType: 'entityAction',
	manifest: {
		type: 'entityAction',
		kind: 'move',
		api: UmbMoveEntityAction,
		weight: 700,
		forEntityTypes: [],
		meta: {
			icon: 'icon-enter',
			label: 'Move to (TBD)...',
			itemRepositoryAlias: '',
			moveRepositoryAlias: '',
			pickerModalAlias: '',
		},
	},
};
