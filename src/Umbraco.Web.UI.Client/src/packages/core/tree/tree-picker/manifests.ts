import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.TreePicker',
		name: 'Tree Picker Modal',
		element: () => import('./tree-picker-modal.element.js'),
	},
];
