import { UMB_BLOCK_LIST_PROPERTY_EDITOR_UI_ALIAS } from '../../property-editors/block-list-editor/constants.js';
import { UMB_WRITABLE_PROPERTY_CONDITION_ALIAS } from '@umbraco-cms/backoffice/property';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyAction',
		kind: 'pasteFromClipboard',
		alias: 'Umb.PropertyEditorUi.BlockList.PasteFromClipboard',
		name: 'Block List Paste From Clipboard Property Action',
		forPropertyEditorUis: [UMB_BLOCK_LIST_PROPERTY_EDITOR_UI_ALIAS],
		conditions: [
			{
				alias: UMB_WRITABLE_PROPERTY_CONDITION_ALIAS,
			},
		],
	},
	{
		type: 'clipboardPasteTranslator',
		alias: 'Umb.ClipboardPasteTranslator.BlockList',
		name: 'Block List Clipboard Paste Translator',
		api: () => import('./paste-translator.js'),
		forClipboardEntryTypes: ['block'],
		forPropertyEditorUiAliases: [UMB_BLOCK_LIST_PROPERTY_EDITOR_UI_ALIAS],
	},
];
