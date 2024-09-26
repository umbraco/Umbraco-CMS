import type { ManifestWorkspaceView, MetaWorkspaceView } from '@umbraco-cms/backoffice/workspace';

export interface ManifestWorkspaceViewCollectionKind extends ManifestWorkspaceView {
	type: 'workspaceView';
	kind: 'collection';
	meta: MetaWorkspaceViewCollectionKind;
}

export interface MetaWorkspaceViewCollectionKind extends MetaWorkspaceView {
	collectionAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestWorkspaceViewCollectionKind: ManifestWorkspaceViewCollectionKind;
	}
}
