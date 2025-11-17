import type { ManifestPropertyContext, MetaPropertyContext } from '@umbraco-cms/backoffice/property';

export interface ManifestPropertyContextSortKind extends ManifestPropertyContext<MetaPropertyContextSortKind> {
	type: 'propertyContext';
	kind: 'sort';
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaPropertyContextSortKind extends MetaPropertyContext {}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestPropertyContextSortKind: ManifestPropertyContextSortKind;
	}
}
