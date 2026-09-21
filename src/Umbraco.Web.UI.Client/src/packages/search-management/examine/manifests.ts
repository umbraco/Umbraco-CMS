import { UMB_SEARCH_DOCUMENT_FIELDS_MODAL_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.SearchDocument.ShowFields',
		name: 'Umbraco Search Provider Examine - Show Fields',
		weight: 100,
		api: () => import('./show-fields.entity-action.js'),
		forEntityTypes: ['search-document'],
		meta: {
			icon: 'icon-search',
			label: '#searchExamine_showFields',
			additionalOptions: false,
		},
		conditions: [
			{
				alias: 'Umb.Search.Condition.IndexProviderName',
				match: 'search-examine-provider',
			},
		],
	},
	{
		type: 'modal',
		alias: UMB_SEARCH_DOCUMENT_FIELDS_MODAL_ALIAS,
		name: 'Umbraco Search Provider Examine - Fields Modal',
		element: () => import('./show-fields.modal.js'),
	},
	{
		type: 'searchIndexDetailBox',
		alias: 'Umb.SearchIndexDetailBox.ExamineFieldsRoute',
		name: 'Umbraco Search Examine Fields Route Provider',
		weight: 0,
		element: () => import('./fields-route-provider.element.js'),
	},
];
