import { UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS } from '../../property-editors/constants.js';
import { UMB_WRITABLE_PROPERTY_CONDITION_ALIAS } from '@umbraco-cms/backoffice/property';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_PROPERTY_ACTION_PASTE_FROM_CLIPBOARD_KIND_MANIFEST } from '@umbraco-cms/backoffice/clipboard';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		...UMB_PROPERTY_ACTION_PASTE_FROM_CLIPBOARD_KIND_MANIFEST.manifest,
		type: 'propertyAction',
		alias: 'Umb.PropertyAction.BlockGrid.Clipboard.Paste',
		name: 'Block Grid Paste From Clipboard Property Action',
		api: () => import('./block-grid-paste-from-clipboard.js'),
		forPropertyEditorUis: [UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS],
		conditions: [
			{
				alias: UMB_WRITABLE_PROPERTY_CONDITION_ALIAS,
			},
		],
	},
];
