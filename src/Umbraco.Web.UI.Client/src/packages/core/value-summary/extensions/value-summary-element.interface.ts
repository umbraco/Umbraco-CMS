import type { UmbValueSummaryApi } from './value-summary-api.interface.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export interface UmbValueSummaryElement extends UmbControllerHostElement {
	api?: UmbValueSummaryApi;
}
