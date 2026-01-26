import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.CollectionAction.RecycleBin.Empty',
	matchKind: 'emptyRecycleBin',
	matchType: 'collectionAction',
	manifest: {
		type: 'collectionAction',
		kind: 'emptyRecycleBin',
		api: () => import('./empty-recycle-bin.collection-action.js'),
		elementName: 'umb-collection-action-button',
		weight: 100,
		meta: {
			label: '#actions_emptyrecyclebin',
		},
	},
};
