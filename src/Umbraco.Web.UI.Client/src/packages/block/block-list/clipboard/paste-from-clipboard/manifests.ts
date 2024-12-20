import { UMB_BLOCK_LIST_PROPERTY_EDITOR_UI_ALIAS } from '../../property-editors/block-list-editor/constants.js';
import { UMB_WRITABLE_PROPERTY_CONDITION_ALIAS } from '@umbraco-cms/backoffice/property';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyAction',
		kind: 'pasteFromClipboard',
		alias: 'Umb.PropertyEditorUi.BlockList.PasteFromClipboard',
		name: 'Block List Paste From Clipboard Property Action',
		forPropertyEditorUis: [UMB_BLOCK_LIST_PROPERTY_EDITOR_UI_ALIAS],
		meta: {
			clipboardPasteResolverAlias: 'Umb.ClipBoardPasteResolver.BlockList',
		},
		conditions: [
			{
				alias: UMB_WRITABLE_PROPERTY_CONDITION_ALIAS,
			},
		],
	},
	{
		type: 'clipboardPasteResolver',
		alias: 'Umb.ClipBoardPasteResolver.BlockList',
		name: 'Block List Clipboard Paste Resolver',
		api: () => import('./paste-resolver.js'),
	},
	{
		type: 'pasteClipboardEntryTranslator',
		alias: 'Umb.PasteClipboardEntryTranslator.BlockToBlockList',
		name: 'Block To Block List Paste Clipboard Entry Translator',
		api: () => import('./paste-block-translator.js'),
		forClipboardEntryTypes: ['block'],
		meta: {
			propertyEditorUiAlias: UMB_BLOCK_LIST_PROPERTY_EDITOR_UI_ALIAS,
		},
	},
];
