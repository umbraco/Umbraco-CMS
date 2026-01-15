import type { ManifestWorkspaceView, MetaWorkspaceView } from '@umbraco-cms/backoffice/workspace';

export interface ManifestWorkspaceViewContentCollectionKind extends ManifestWorkspaceView<MetaWorkspaceView> {
	type: 'workspaceView';
	kind: 'contentCollection';
}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestWorkspaceViewContentCollectionKind: ManifestWorkspaceViewContentCollectionKind;
	}
}
