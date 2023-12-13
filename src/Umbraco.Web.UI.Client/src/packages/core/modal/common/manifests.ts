import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.Confirm',
		name: 'Confirm Modal',
		js: () => import('./confirm/confirm-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.Folder.Update',
		name: 'Update Folder Modal',
		js: () => import('./folder/folder-update-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.Folder.Create',
		name: 'Create Folder Modal',
		js: () => import('./folder/folder-create-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.IconPicker',
		name: 'Icon Picker Modal',
		js: () => import('./icon-picker/icon-picker-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.LinkPicker',
		name: 'Link Picker Modal',
		js: () => import('./link-picker/link-picker-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.PropertySettings',
		name: 'Property Settings Modal',
		js: () => import('./property-settings/property-settings-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.SectionPicker',
		name: 'Section Picker Modal',
		js: () => import('./section-picker/section-picker-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.Template',
		name: 'Template Modal',
		js: () => import('./template/template-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.CodeEditor',
		name: 'Code Editor Modal',
		js: () => import('./code-editor/code-editor-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.EmbeddedMedia',
		name: 'Embedded Media Modal',
		js: () => import('./embedded-media/embedded-media-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.TreePicker',
		name: 'Tree Picker Modal',
		js: () => import('./tree-picker/tree-picker-modal.element.js'),
	},
];

export const manifests = [...modals];
