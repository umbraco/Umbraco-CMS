import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { ManifestElementAndApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbRouteEntry } from '@umbraco-cms/backoffice/router';

export interface ManifestSectionRoute extends ManifestElementAndApi<UmbControllerHostElement, UmbRouteEntry> {
	type: 'sectionRoute';
	meta: MetaSectionRoute;
}

export interface MetaSectionRoute {
	path?: string;
}
