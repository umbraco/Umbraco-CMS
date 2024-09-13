import type { UmbWorkspaceViewElement } from '../interfaces/workspace-view-element.interface.js';
import type {
	ManifestWithDynamicConditions,
	ManifestWithView,
	MetaManifestWithView,
} from '@umbraco-cms/backoffice/extension-api';

export interface ManifestWorkspaceView<MetaType extends MetaWorkspaceView = MetaWorkspaceView>
	extends ManifestWithView<UmbWorkspaceViewElement>,
		ManifestWithDynamicConditions<UmbExtensionCondition> {
	type: 'workspaceView';
	meta: MetaType;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaWorkspaceView extends MetaManifestWithView {}

export interface ManifestWorkspaceViewCollectionKind extends ManifestWorkspaceView<MetaWorkspaceView> {
	type: 'workspaceView';
	kind: 'collection';
}

export interface ManifestWorkspaceViewContentTypeDesignEditorKind extends ManifestWorkspaceView {
	type: 'workspaceView';
	kind: 'contentTypeDesignEditor';
	meta: MetaWorkspaceViewContentTypeDesignEditorKind;
}

export interface MetaWorkspaceViewContentTypeDesignEditorKind extends MetaWorkspaceView {
	compositionRepositoryAlias?: string;
}
