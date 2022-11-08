import type { ManifestElement } from './models';

export interface ManifestEditor extends ManifestElement {
	type: 'editor';
	meta: MetaEditor;
}

export interface MetaEditor {
	entityType: string;
}
