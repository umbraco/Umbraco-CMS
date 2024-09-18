import type { UmbContentTypeModel } from '@umbraco-cms/backoffice/content-type';
import type {
	ManifestWorkspaceView,
	MetaWorkspaceView,
	UmbCollectionWorkspaceContext,
} from '@umbraco-cms/backoffice/workspace';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbContentCollectionWorkspaceContext<ContentType extends UmbContentTypeModel>
	extends UmbCollectionWorkspaceContext<ContentType> {}

export interface ManifestWorkspaceViewContentCollectionKind extends ManifestWorkspaceView<MetaWorkspaceView> {
	type: 'workspaceView';
	kind: 'contentCollection';
}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestWorkspaceViewContentCollectionKind: ManifestWorkspaceViewContentCollectionKind;
	}
}
