import type { ManifestPropertyContext, MetaPropertyContext } from '@umbraco-cms/backoffice/property';

export interface ManifestPropertyContextClipboardKind
	extends ManifestPropertyContext<MetaPropertyContextClipboardKind> {
	type: 'propertyContext';
	kind: 'clipboard';
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaPropertyContextClipboardKind extends MetaPropertyContext {}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestPropertyContextClipboardKind: ManifestPropertyContextClipboardKind;
	}
}
