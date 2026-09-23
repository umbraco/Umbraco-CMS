import { UMB_SEARCH_WORKSPACE_ALIAS } from '../../constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Search.Details',
		name: 'Search Details View',
		element: () => import('./search-details-view.element.js'),
		weight: 300,
		meta: {
			label: '#general_details',
			pathname: 'details',
			icon: 'icon-search',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_SEARCH_WORKSPACE_ALIAS,
			},
		],
	},
];
