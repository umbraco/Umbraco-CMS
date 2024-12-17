import { UMB_BLOCK_LIST_PROPERTY_EDITOR_UI_ALIAS } from '../../constants.js';
import { UMB_WRITABLE_PROPERTY_CONDITION_ALIAS } from '@umbraco-cms/backoffice/property';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyAction',
		kind: 'copyToClipboard',
		alias: 'Umb.PropertyEditorUi.BlockList.CopyToClipboard',
		name: 'Block List Copy To Clipboard Property Action',
		forPropertyEditorUis: [UMB_BLOCK_LIST_PROPERTY_EDITOR_UI_ALIAS],
		meta: {
			entry: {
				type: 'blockList',
			},
		},
		conditions: [
			{
				alias: UMB_WRITABLE_PROPERTY_CONDITION_ALIAS,
			},
		],
	},
];
