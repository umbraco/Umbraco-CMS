import type { ManifestApi, UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestWorkspace extends ManifestApi<UmbApi> {
	type: 'workspaceContext';
	meta: MetaEditor;
}

export interface MetaEditor {
	entityType: string;
}
