import type { ManifestModal } from '@umbraco-cms/extensions-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.IconPicker',
		name: 'Icon Picker Modal',
		loader: () => import('./icon-picker/icon-picker-modal.element'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.LinkPicker',
		name: 'Link Picker Modal',
		loader: () => import('./link-picker/link-picker-modal.element'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.Basic',
		name: 'Basic Modal',
		loader: () => import('./basic/basic-modal.element'),
	},
];

export const manifests = [...modals];
