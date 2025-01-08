import type { ManifestPropertyAction, MetaPropertyAction } from '@umbraco-cms/backoffice/property-action';

export interface ManifestPropertyActionPasteFromClipboardKind
	extends ManifestPropertyAction<MetaPropertyActionPasteFromClipboardKind> {
	type: 'propertyAction';
	kind: 'pasteFromClipboard';
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaPropertyActionPasteFromClipboardKind extends MetaPropertyAction {}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestPropertyActionPasteFromClipboardKind: ManifestPropertyActionPasteFromClipboardKind;
	}
}
