import type { UmbRouteEntry } from '../../router/types.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { ManifestElementAndApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestSectionRoute extends ManifestElementAndApi<UmbControllerHostElement, UmbRouteEntry> {
	type: 'sectionRoute';
	meta: MetaSectionRoute;
}

export interface MetaSectionRoute {
	path?: string;
}
