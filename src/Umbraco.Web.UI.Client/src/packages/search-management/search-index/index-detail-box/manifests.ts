import type { ManifestSearchIndexDetailBox } from './types.js';
import type { ManifestKind } from '@umbraco-cms/backoffice/extension-api';

const kind: ManifestKind<ManifestSearchIndexDetailBox> = {
	type: 'kind',
	alias: 'Umb.Kind.SearchIndexDetailBox',
	matchKind: 'default',
	matchType: 'searchIndexDetailBox',
	manifest: {
		type: 'searchIndexDetailBox',
		kind: 'default',
	},
};

export const manifests: Array<UmbExtensionManifest> = [
	kind as never,
	{
		type: 'searchIndexDetailBox',
		alias: 'Umb.SearchIndexDetailBox.Stats',
		name: 'Search Index Stats Box',
		weight: 100,
		element: () => import('./search-index-stats-box.element.js'),
		meta: {
			label: '#searchManagement_statsBoxLabel',
			column: 'right',
		},
	},
	{
		type: 'searchIndexDetailBox',
		alias: 'Umb.SearchIndexDetailBox.Search',
		name: 'Search Index Search Box',
		weight: 100,
		element: () => import('./search-index-search-box.element.js'),
		meta: {
			label: '#searchManagement_searchBoxLabel',
			column: 'left',
		},
	},
];
