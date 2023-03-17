import type { ManifestTypes } from '@umbraco-cms/models';

const headerApps: Array<ManifestTypes> = [
	{
		type: 'headerApp',
		alias: 'Umb.HeaderApp.Search',
		name: 'Header App Search',
		loader: () => import('./umb-search-header-app.element'),
		weight: 10,
		meta: {
			label: 'Search',
			icon: 'search',
			pathname: 'search',
		},
	},

	{
		type: 'headerApp',
		kind: 'Umb.Kind.Button',
		alias: 'Umb.HeaderApp.HackDemo',
		name: 'Header App Search',
		weight: 10,
		meta: {
			label: 'Hack Demo',
			icon: 'document',
			href: '/section/content/workspace/document/edit/c05da24d-7740-447b-9cdc-bd8ce2172e38/en-us/view/content/tab/Local%20blog%20tab',
		},
	},
];

export const manifests = [...headerApps];
