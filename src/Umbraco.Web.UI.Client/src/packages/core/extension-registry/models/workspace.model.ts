import type { ManifestElementAndApi, UmbApi } from '@umbraco-cms/backoffice/extension-api';

// TODO: Missing Extension API Interface:
export interface ManifestWorkspace extends ManifestElementAndApi<HTMLElement, UmbApi> {
	type: 'workspace';
	meta: MetaWorkspace;
}

export interface MetaWorkspace {
	entityType: string;
}
