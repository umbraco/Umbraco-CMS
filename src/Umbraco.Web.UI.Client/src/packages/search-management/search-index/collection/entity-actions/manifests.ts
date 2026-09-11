import { UMB_SEARCH_INDEX_ENTITY_TYPE } from '../../constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.Search.EntityAction.RebuildIndex',
		name: 'Umbraco Search Entity Action - Rebuild Index',
		weight: 100,
		api: () => import('./rebuild-index.action.js'),
		forEntityTypes: [UMB_SEARCH_INDEX_ENTITY_TYPE],
		meta: {
			icon: 'icon-refresh',
			label: '#searchManagement_entityActionRebuildIndex',
			additionalOptions: false,
		},
	},
];
