import { UmbCopyEntityAction } from './copy.action.js';
import type { UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbBackofficeManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.Copy',
	matchKind: 'copy',
	matchType: 'entityAction',
	manifest: {
		type: 'entityAction',
		kind: 'copy',
		api: UmbCopyEntityAction,
		weight: 900,
		meta: {
			icon: 'icon-trash',
			label: 'Delete...',
			entityTypes: [],
			itemRepositoryAlias: '',
			copyRepositoryAlias: '',
			pickerModalAlias: '',
		},
	},
};
