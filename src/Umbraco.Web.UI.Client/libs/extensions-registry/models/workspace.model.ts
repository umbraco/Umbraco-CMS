import type { ManifestElement } from '.';

export interface ManifestWorkspace extends ManifestElement {
	type: 'workspace';
	meta: MetaEditor;
}

export interface MetaEditor {
	entityType: string;
}
