import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.Confirm',
		name: 'Confirm Modal',
		loader: () => import('./confirm/confirm-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.Folder',
		name: 'Folder Modal',
		loader: () => import('./folder/folder-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.IconPicker',
		name: 'Icon Picker Modal',
		loader: () => import('./icon-picker/icon-picker-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.LinkPicker',
		name: 'Link Picker Modal',
		loader: () => import('./link-picker/link-picker-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.PropertySettings',
		name: 'Property Settings Modal',
		loader: () => import('./property-settings/property-settings-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.SectionPicker',
		name: 'Section Picker Modal',
		loader: () => import('./section-picker/section-picker-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.Template',
		name: 'Template Modal',
		loader: () => import('./template/template-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.EmbeddedMedia',
		name: 'Embedded Media Modal',
		loader: () => import('./embedded-media/embedded-media-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.TreePicker',
		name: 'Tree Picker Modal',
		loader: () => import('./tree-picker/tree-picker-modal.element.js'),
	},
];

export const manifests = [...modals];
