import type { ManifestHeaderApp } from '@umbraco-cms/models';

const headerApps: Array<ManifestHeaderApp> = [
	{
		type: 'headerApp',
		alias: 'Umb.HeaderApp.Search',
		name: 'Header App Search',
		loader: () => import('src/backoffice/core/components/header-app/header-app-button.element'),
		weight: 10,
		meta: {
			label: 'Search',
			icon: 'search',
			pathname: 'search',
		},
	},
];

export const manifests = [...headerApps];
