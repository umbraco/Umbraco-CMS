import type {
	ManifestWorkspaceActionMenuItem,
	MetaWorkspaceActionMenuItemDefaultKind,
} from '@umbraco-cms/backoffice/workspace';

export interface ManifestWorkspaceActionMenuItemPreviewOptionKind
	extends ManifestWorkspaceActionMenuItem<MetaWorkspaceActionMenuItemPreviewOptionKind> {
	type: 'workspaceActionMenuItem';
	kind: 'previewOption';
}

export interface MetaWorkspaceActionMenuItemPreviewOptionKind extends MetaWorkspaceActionMenuItemDefaultKind {
	urlProviderAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbWorkspaceActionMenuItemPreviewOptionKind: ManifestWorkspaceActionMenuItemPreviewOptionKind;
	}
}
