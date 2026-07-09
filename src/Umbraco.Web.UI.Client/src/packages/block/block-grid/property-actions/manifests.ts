import { UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS } from '../constants.js';
import {
	UMB_PROPERTY_HAS_VALUE_CONDITION_ALIAS,
	UMB_WRITABLE_PROPERTY_CONDITION_ALIAS,
} from '@umbraco-cms/backoffice/property';
import { UMB_PROPERTY_ACTION_PASTE_FROM_CLIPBOARD_KIND_MANIFEST } from '@umbraco-cms/backoffice/clipboard';

const clipboardPaste: UmbExtensionManifest = {
	...UMB_PROPERTY_ACTION_PASTE_FROM_CLIPBOARD_KIND_MANIFEST.manifest,
	type: 'propertyAction',
	alias: 'Umb.PropertyAction.BlockGrid.Clipboard.Paste',
	name: 'Block Grid Paste From Clipboard Property Action',
	api: () => import('./block-grid-paste-from-clipboard.js'),
	forPropertyEditorUis: [UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS],
	conditions: [{ alias: UMB_WRITABLE_PROPERTY_CONDITION_ALIAS }],
};

const sortMode: UmbExtensionManifest = {
	type: 'propertyAction',
	kind: 'sortMode',
	alias: 'Umb.PropertyAction.BlockGrid.SortMode',
	name: 'Block Grid Sort Mode Property Action',
	forPropertyEditorUis: [UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS],
	conditions: [
		{
			alias: UMB_PROPERTY_HAS_VALUE_CONDITION_ALIAS,
		},
	],
};

const clear: UmbExtensionManifest = {
	type: 'propertyAction',
	kind: 'clear',
	alias: 'Umb.PropertyAction.BlockGrid.Clear',
	name: 'Block Grid Clear Property Action',
	forPropertyEditorUis: [UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS],
};

export const manifests: Array<UmbExtensionManifest> = [clipboardPaste, sortMode, clear];
