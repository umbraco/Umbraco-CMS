import { UMB_COLLECTION_ITEM_PICKER_MODAL_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: UMB_COLLECTION_ITEM_PICKER_MODAL_ALIAS,
		name: 'Collection Item Picker Modal',
		element: () => import('./collection-item-picker-modal.element.js'),
	},
];
