import type { ManifestModal } from '@umbraco-cms/backoffice/modal';

export const manifests: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.TemplatingItemPicker',
		name: 'Templating Item Picker Modal',
		element: () => import('./templating-item-picker/templating-item-picker-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.TemplatingSectionPicker',
		name: 'Templating Section Picker Modal',
		element: () => import('./templating-section-picker/templating-section-picker-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.TemplatingPageFieldBuilder',
		name: 'Templating Page Field Builder Modal',
		element: () => import('./templating-page-field-builder/templating-page-field-builder-modal.element.js'),
	},
];
