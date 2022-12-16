import type { ManifestElement } from './models';

export interface ManifestWorkspaceView extends ManifestElement {
	type: 'workspaceView';
	meta: MetaEditorView;
}

export interface MetaEditorView {
	workspaces: string[];
	pathname: string;
	label: string;
	icon: string;
}
