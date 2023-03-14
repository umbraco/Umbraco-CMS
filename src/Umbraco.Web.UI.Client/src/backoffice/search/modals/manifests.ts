import type { ManifestModal } from '@umbraco-cms/extensions-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.Search',
		name: 'Search Modal',
		loader: () => import('./search/search-modal.element'),
	},
];

export const manifests = [...modals];
