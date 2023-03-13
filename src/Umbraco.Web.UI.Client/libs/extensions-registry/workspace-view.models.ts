import type { ManifestWithView } from './models';

export interface ManifestWorkspaceView extends ManifestWithView {
	type: 'workspaceView';
	meta: MetaEditorView;
	conditions: ConditionsEditorView;
}

export interface MetaEditorView {
	pathname: string;
	label: string;
	icon: string;
}

export interface ConditionsEditorView {
	workspaces: string[];
}
