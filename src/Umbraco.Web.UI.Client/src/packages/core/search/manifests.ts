import UmbSearchHeaderAppElement from './umb-search-header-app.element.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'headerApp',
		alias: 'Umb.HeaderApp.Search',
		name: 'Header App Search',
		element: UmbSearchHeaderAppElement,
		weight: 900,
		meta: {
			label: 'Search',
			icon: 'search',
			pathname: 'search',
		},
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.Search',
		name: 'Search Modal',
		element: () => import('./search-modal/search-modal.element.js'),
	},
];
