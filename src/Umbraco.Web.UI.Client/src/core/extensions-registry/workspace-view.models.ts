import type { ManifestElement } from './models';

export interface ManifestWorkspaceView extends ManifestElement {
	type: 'workspaceView';
	meta: MetaEditorView;
}

export interface MetaEditorView {
	editors: string[];
	pathname: string;
	label: string;
	icon: string;
}
