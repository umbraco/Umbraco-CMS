export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.CollectionItemPicker',
		name: 'Collection Item Picker Modal',
		element: () => import('./collection-item-picker-modal.element.js'),
	},
];
