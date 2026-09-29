import UmbItemPickerModalElement from './item-picker-modal.element.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.ItemPicker',
		name: 'Item Picker Modal',
		element: UmbItemPickerModalElement,
	},
];
