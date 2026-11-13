import type { UmbFilterApi } from './filter-api.interface.js';
import type { ManifestFilterBase } from './filter.extension.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export interface UmbFilterElement extends UmbControllerHostElement {
	api?: UmbFilterApi;
	manifest?: ManifestFilterBase;
}
