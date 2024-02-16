import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.TemplatingItemPicker',
		name: 'Templating Item Picker Modal',
		js: () => import('./templating-item-picker/templating-item-picker-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.TemplatingSectionPicker',
		name: 'Templating Section Picker Modal',
		js: () => import('./templating-section-picker/templating-section-picker-modal.element.js'),
	},
];

export const manifests = [...modals];
