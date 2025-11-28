import type { ManifestPropertyAction, MetaPropertyActionDefaultKind } from '@umbraco-cms/backoffice/property-action';

export interface ManifestPropertyActionSortModeKind<
	MetaType extends MetaPropertyActionSortModeKind = MetaPropertyActionSortModeKind,
> extends ManifestPropertyAction<MetaType> {
	type: 'propertyAction';
	kind: 'sortMode';
}

export type MetaPropertyActionSortModeKind = MetaPropertyActionDefaultKind;

declare global {
	interface UmbExtensionManifestMap {
		umbManifestPropertyActionSortModeKind: ManifestPropertyActionSortModeKind;
	}
}
