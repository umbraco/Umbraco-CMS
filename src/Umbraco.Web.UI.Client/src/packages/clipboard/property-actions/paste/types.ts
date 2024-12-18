import type { ManifestPropertyAction, MetaPropertyAction } from '@umbraco-cms/backoffice/property-action';

export interface ManifestPropertyActionPasteFromClipboardKind
	extends ManifestPropertyAction<MetaPropertyActionPasteFromClipboardKind> {
	type: 'propertyAction';
	kind: 'pasteFromClipboard';
}

export interface MetaPropertyActionPasteFromClipboardKind extends MetaPropertyAction {
	entry: {
		type: string;
	};
	clipboardPasteResolverAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestPropertyActionPasteFromClipboardKind: ManifestPropertyActionPasteFromClipboardKind;
	}
}
