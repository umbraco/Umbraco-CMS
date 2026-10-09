import UmbTreePickerModalElement from './tree-picker-modal.element.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.TreePicker',
		name: 'Tree Picker Modal',
		element: UmbTreePickerModalElement,
	},
];
