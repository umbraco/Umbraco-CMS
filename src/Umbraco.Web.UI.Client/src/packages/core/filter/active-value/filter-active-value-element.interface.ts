import type { UmbFilterActiveValueApi } from './filter-active-value-api.interface.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export interface UmbFilterActiveValueElement<
	TValue = unknown,
	TResolvedValue = unknown,
> extends UmbControllerHostElement {
	api?: UmbFilterActiveValueApi<TValue, TResolvedValue>;
}
