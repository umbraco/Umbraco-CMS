import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestWorkspace extends ManifestElement {
	type: 'workspace';
	meta: MetaEditor;
}

export interface MetaEditor {
	entityType: string;
}
