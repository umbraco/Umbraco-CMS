import type { ManifestWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export interface ManifestWorkspaceContextMenuStructureKind extends ManifestWorkspaceContext {
	type: 'workspaceContext';
	kind: 'menuStructure';
	meta: MetaWorkspaceContextMenuStructureKind;
}

export interface MetaWorkspaceContextMenuStructureKind {
	menuItemAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestWorkspaceContextMenuStructureKind: ManifestWorkspaceContextMenuStructureKind;
	}
}
