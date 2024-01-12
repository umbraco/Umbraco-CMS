import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.BlockCatalogue',
		name: 'Block Catalogue Modal',
		js: () => import('./block-catalogue/block-catalogue-modal.element.js'),
	},
];

export const manifests = [...modals];
