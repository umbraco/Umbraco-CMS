import { UMB_CLIPBOARD_ENTRY_PICKER_MODAL_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: UMB_CLIPBOARD_ENTRY_PICKER_MODAL_ALIAS,
		name: 'Clipboard Item Picker Modal',
		js: () => import('./clipboard-entry-picker-modal.element.js'),
	},
];
