import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

/**
 * Per-element API for value summary extensions.
 * Set on `element.api` by `umb-extension-with-api-slot`.
 * Elements observe `value` to get the display value.
 */
export interface UmbValueSummaryApi extends UmbApi {
	readonly value: Observable<unknown>;
}
