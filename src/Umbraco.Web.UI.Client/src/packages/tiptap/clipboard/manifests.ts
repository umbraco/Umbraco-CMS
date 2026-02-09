import { UMB_TIPTAP_PROPERTY_EDITOR_UI_ALIAS } from '../property-editors/tiptap-rte/constants.js';
import { manifest as blockCopy } from './block/copy/manifest.js';
import { manifest as blockPaste } from './block/paste/manifest.js';

const propertyContext: UmbExtensionManifest = {
	type: 'propertyContext',
	kind: 'clipboard',
	alias: 'Umb.PropertyContext.Tiptap.Clipboard',
	name: 'Tiptap Clipboard Property Context',
	forPropertyEditorUis: [UMB_TIPTAP_PROPERTY_EDITOR_UI_ALIAS],
};

export const manifests: Array<UmbExtensionManifest> = [blockCopy, blockPaste, propertyContext];
