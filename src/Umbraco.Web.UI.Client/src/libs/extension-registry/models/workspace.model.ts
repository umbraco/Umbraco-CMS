import type { ManifestElement } from 'src/libs/extension-api';

export interface ManifestWorkspace extends ManifestElement {
	type: 'workspace';
	meta: MetaEditor;
}

export interface MetaEditor {
	entityType: string;
}
