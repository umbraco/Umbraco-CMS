import type { UmbWorkspaceEditorViewExtensionElement } from '../interfaces/workspace-editor-view-extension-element.interface';
import type { ManifestWithView } from 'src/libs/extension-api';

export interface ManifestWorkspaceEditorView extends ManifestWithView<UmbWorkspaceEditorViewExtensionElement> {
	type: 'workspaceEditorView';
	conditions: ConditionsWorkspaceView;
}

export interface ConditionsWorkspaceView {
	workspaces: string[];
}
