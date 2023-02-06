import type { ManifestWithView } from './models';

export interface ManifestWorkspaceView extends ManifestWithView {
	type: 'workspaceView';
	meta: MetaEditorView;
}

export interface MetaEditorView {
	workspaces: string[];
	pathname: string;
	label: string;
	icon: string;
}
