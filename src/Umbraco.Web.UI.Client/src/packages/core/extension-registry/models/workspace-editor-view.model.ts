import type { UmbWorkspaceEditorViewExtensionElement } from '../interfaces/workspace-editor-view-extension-element.interface.js';
import type { ManifestWithView } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestWorkspaceEditorView extends ManifestWithView<UmbWorkspaceEditorViewExtensionElement> {
	type: 'workspaceEditorView';
	conditions: ConditionsWorkspaceView;
}

export interface ConditionsWorkspaceView {
	workspaces: string[];
}
