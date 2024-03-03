import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { ManifestElementAndApi, UmbApi } from '@umbraco-cms/backoffice/extension-api';

// TODO: Missing Extension API Interface:
export interface ManifestWorkspace extends ManifestElementAndApi<UmbControllerHostElement, UmbApi> {
	type: 'workspace';
	meta: MetaWorkspace;
}

export interface MetaWorkspace {
	entityType: string;
}
