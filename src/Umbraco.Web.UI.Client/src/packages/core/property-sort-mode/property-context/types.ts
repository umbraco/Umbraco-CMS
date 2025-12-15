import type { ManifestPropertyContext, MetaPropertyContext } from '@umbraco-cms/backoffice/property';

export interface ManifestPropertyContextSortModeKind extends ManifestPropertyContext<MetaPropertyContextSortModeKind> {
	type: 'propertyContext';
	kind: 'sortMode';
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaPropertyContextSortModeKind extends MetaPropertyContext {}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestPropertyContextSortModeKind: ManifestPropertyContextSortModeKind;
	}
}
