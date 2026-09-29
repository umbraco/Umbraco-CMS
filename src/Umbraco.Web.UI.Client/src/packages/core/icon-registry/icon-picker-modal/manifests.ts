import UmbIconPickerModalElement from './icon-picker-modal.element.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.IconPicker',
		name: 'Icon Picker Modal',
		element: UmbIconPickerModalElement,
	},
];
