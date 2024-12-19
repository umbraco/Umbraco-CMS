import type { ManifestPropertyAction, MetaPropertyAction } from '@umbraco-cms/backoffice/property-action';

export interface ManifestPropertyActionCopyToClipboardKind
	extends ManifestPropertyAction<MetaPropertyActionCopyToClipboardKind> {
	type: 'propertyAction';
	kind: 'copyToClipboard';
}

export interface MetaPropertyActionCopyToClipboardKind extends MetaPropertyAction {
	clipboardCopyResolverAlias?: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestPropertyActionCopyToClipboardKind: ManifestPropertyActionCopyToClipboardKind;
	}
}
