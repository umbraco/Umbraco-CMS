import type {
	ManifestWithDynamicConditions,
	ManifestWithView,
	MetaManifestWithView,
} from '@umbraco-cms/backoffice/extension-api';

export interface UmbWorkspaceViewElement extends HTMLElement {
	manifest?: ManifestWorkspaceView;
}

export interface ManifestWorkspaceView<MetaType extends MetaWorkspaceView = MetaWorkspaceView>
	extends ManifestWithView<UmbWorkspaceViewElement>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'workspaceView';
	meta: MetaType;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaWorkspaceView extends MetaManifestWithView {}

export interface ManifestWorkspaceViewContentTypeDesignEditorKind extends ManifestWorkspaceView {
	type: 'workspaceView';
	kind: 'contentTypeDesignEditor';
	meta: MetaWorkspaceViewContentTypeDesignEditorKind;
}

export interface MetaWorkspaceViewContentTypeDesignEditorKind extends MetaWorkspaceView {
	compositionRepositoryAlias?: string;
}

declare global {
	interface UmbExtensionManifestMap {
		ManifestWorkspaceView: ManifestWorkspaceView;
		ManifestWorkspaceViewContentTypeDesignEditorKind: ManifestWorkspaceViewContentTypeDesignEditorKind;
	}
}
