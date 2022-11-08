import type { ManifestElement } from './models';

export interface ManifestEditorView extends ManifestElement {
	type: 'editorView';
	meta: MetaEditorView;
}

export interface MetaEditorView {
	editors: string[];
	pathname: string;
	label: string;
	icon: string;
}
