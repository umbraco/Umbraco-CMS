import type {
	ManifestWorkspaceActionMenuItem,
	MetaWorkspaceActionMenuItemDefaultKind,
} from '@umbraco-cms/backoffice/workspace';

export interface ManifestWorkspaceActionMenuItemUrlProviderKind<
	MetaType extends MetaWorkspaceActionMenuItemDefaultKind = MetaWorkspaceActionMenuItemDefaultKind,
> extends ManifestWorkspaceActionMenuItem<MetaType> {
	type: 'workspaceActionMenuItem';
	kind: 'urlProvider';
	urlProviderAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbWorkspaceActionMenuItemUrlProviderKind: ManifestWorkspaceActionMenuItemUrlProviderKind;
	}
}
