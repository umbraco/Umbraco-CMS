import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.CompositionPicker',
		name: 'Block Catalogue Modal',
		js: () => import('./composition-picker/composition-picker-modal.element.js'),
	},
];

export const manifests = [...modals];
