import type { ManifestElement } from './models';

export interface ManifestWorkspaceAction extends ManifestElement {
	type: 'workspaceAction';
	meta: MetaEditorAction;
}

export interface MetaEditorAction {
	editors: Array<string>;
}
