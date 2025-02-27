export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.ItemPicker',
		name: 'Item Picker Modal',
		element: () => import('./item-picker-modal.element.js'),
	},
];
