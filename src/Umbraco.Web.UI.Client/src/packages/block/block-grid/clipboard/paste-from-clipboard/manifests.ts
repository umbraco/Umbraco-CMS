import { UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS } from '../../property-editors/constants.js';
import { UMB_WRITABLE_PROPERTY_CONDITION_ALIAS } from '@umbraco-cms/backoffice/property';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyAction',
		kind: 'pasteFromClipboard',
		alias: 'Umb.PropertyEditorUi.BlockGrid.PasteFromClipboard',
		name: 'Block Grid Paste From Clipboard Property Action',
		forPropertyEditorUis: [UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS],
		meta: {
			clipboardPasteResolverAlias: 'Umb.ClipBoardPasteResolver.BlockGrid',
		},
		conditions: [
			{
				alias: UMB_WRITABLE_PROPERTY_CONDITION_ALIAS,
			},
		],
	},
	{
		type: 'clipboardPasteResolver',
		alias: 'Umb.ClipBoardPasteResolver.BlockGrid',
		name: 'Block Grid Clipboard Paste Resolver',
		api: () => import('./paste-resolver.js'),
	},
];
