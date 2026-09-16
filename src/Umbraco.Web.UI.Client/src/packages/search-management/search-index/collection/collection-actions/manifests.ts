import { UMB_SEARCH_ROOT_COLLECTION_ALIAS } from '../../constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionAction',
		kind: 'button',
		name: 'Umbraco Search Collection Action - Reload',
		// Kept on the spelling it shipped with in the standalone Umbraco.Cms.Search package.
		// Unlike the repository and store aliases, an extension that renders cannot be registered
		// under a second alias without appearing twice, so this one cannot be renamed and shimmed.
		// eslint-disable-next-line local-rules/enforce-manifest-alias
		alias: 'Umbraco.Search.CollectionAction.Reload',
		api: () => import('./reload.collection-action.js'),
		meta: {
			label: '#searchManagement_collectionActionReload',
		},
		conditions: [
			{
				alias: 'Umb.Condition.CollectionAlias',
				match: UMB_SEARCH_ROOT_COLLECTION_ALIAS,
			},
		],
	},
];
