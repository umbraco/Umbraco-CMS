import type { ManifestElementAndApi, UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestWorkspace extends ManifestElementAndApi<HTMLElement, UmbApi> {
	type: 'workspace';
	meta: MetaEditor;
}

export interface MetaEditor {
	entityType: string;
}
