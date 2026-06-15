import type { ManifestWorkspaceView, MetaWorkspaceView } from '@umbraco-cms/backoffice/workspace';

export interface ManifestWorkspaceViewTreekind extends ManifestWorkspaceView {
	type: 'workspaceView';
	kind: 'tree';
	meta: MetaWorkspaceViewTreeKind;
}

export interface MetaWorkspaceViewTreeKind extends MetaWorkspaceView {
	treeAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestWorkspaceViewTreeKind: ManifestWorkspaceViewTreekind;
	}
}
