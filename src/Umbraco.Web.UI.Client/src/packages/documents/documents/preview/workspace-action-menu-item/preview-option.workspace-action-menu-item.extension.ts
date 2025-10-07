import type {
	ManifestWorkspaceActionMenuItem,
	MetaWorkspaceActionMenuItemDefaultKind,
} from '@umbraco-cms/backoffice/workspace';

export interface ManifestWorkspaceActionMenuItemPreviewOptionKind<
	MetaType extends MetaWorkspaceActionMenuItemDefaultKind = MetaWorkspaceActionMenuItemDefaultKind,
> extends ManifestWorkspaceActionMenuItem<MetaType> {
	type: 'workspaceActionMenuItem';
	kind: 'previewOption';
	urlProviderAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbWorkspaceActionMenuItemPreviewOptionKind: ManifestWorkspaceActionMenuItemPreviewOptionKind;
	}
}
