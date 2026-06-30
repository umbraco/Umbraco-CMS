import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

/**
 * Element API for value summary extensions.
 */
export interface UmbValueSummaryApi<ValueType> extends UmbApi {
	readonly value: Observable<ValueType | undefined>;
}
