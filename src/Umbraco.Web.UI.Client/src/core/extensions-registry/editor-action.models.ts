import type { ManifestElement } from './models';

export interface ManifestEditorAction extends ManifestElement {
	type: 'editorAction';
	meta: MetaEditorAction;
}

export interface MetaEditorAction {
	editors: Array<string>;
}
