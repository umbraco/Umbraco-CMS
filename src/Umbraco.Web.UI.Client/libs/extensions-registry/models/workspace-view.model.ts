import type { UmbWorkspaceEditorViewExtensionElement } from '../interfaces/workspace-editor-view-extension-element.interface';
import type { ManifestWithView } from '.';

export interface ManifestWorkspaceView extends ManifestWithView<UmbWorkspaceEditorViewExtensionElement> {
	type: 'workspaceView';
	conditions: ConditionsWorkspaceView;
}

export interface ConditionsWorkspaceView {
	workspaces: string[];
}
