import type { UmbWorkspaceEditorViewExtensionElement } from '../interfaces/workspace-editor-view-extension-element.interface.js';
import type { ManifestWithView, MetaManifestWithView } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestWorkspaceEditorView extends ManifestWithView<UmbWorkspaceEditorViewExtensionElement> {
	type: 'workspaceEditorView';
	meta: MetaWorkspaceView;
}

export interface MetaWorkspaceView extends MetaManifestWithView {
	workspaces: Array<string>;
}
