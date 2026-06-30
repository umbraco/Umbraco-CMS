import { UMB_BLOCK_ACTION_COPY_TO_CLIPBOARD_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'blockAction',
		kind: 'default',
		alias: UMB_BLOCK_ACTION_COPY_TO_CLIPBOARD_ALIAS,
		name: 'Copy to Clipboard Block Action',
		weight: 200,
		api: () => import('./copy-to-clipboard-block.action.js'),
		meta: {
			icon: 'icon-clipboard-copy',
			label: '#clipboard_labelForCopyToClipboard',
		},
	},
];
