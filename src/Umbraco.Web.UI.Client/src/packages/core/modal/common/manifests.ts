export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.Confirm',
		name: 'Confirm Modal',
		element: () => import('./confirm/confirm-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.CodeEditor',
		name: 'Code Editor Modal',
		element: () => import('./code-editor/code-editor-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.EmbeddedMedia',
		name: 'Embedded Media Modal',
		element: () => import('./embedded-media/embedded-media-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.ItemPicker',
		name: 'Item Picker Modal',
		element: () => import('./item-picker/item-picker-modal.element.js'),
	},
];
