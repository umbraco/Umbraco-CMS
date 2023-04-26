import type { ManifestWithView } from '.';

export interface ManifestWorkspaceView extends ManifestWithView {
	type: 'workspaceView';
	conditions: ConditionsWorkspaceView;
}

export interface ConditionsWorkspaceView {
	workspaces: string[];
}
