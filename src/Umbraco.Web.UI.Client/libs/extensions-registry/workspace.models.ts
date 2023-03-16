import type { ManifestElement } from './models';

export interface ManifestWorkspace extends ManifestElement {
	type: 'workspace';
	meta: MetaEditor;
}

export interface MetaEditor {
	entityType: string;
}
