import type { ManifestPropertyAction, MetaPropertyAction } from '@umbraco-cms/backoffice/property-action';

export interface ManifestPropertyActionReplaceFromClipboardKind
	extends ManifestPropertyAction<MetaPropertyActionReplaceFromClipboardKind> {
	type: 'propertyAction';
	kind: 'replaceFromClipboard';
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaPropertyActionReplaceFromClipboardKind extends MetaPropertyAction {}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestPropertyActionReplaceFromClipboardKind: ManifestPropertyActionReplaceFromClipboardKind;
	}
}
