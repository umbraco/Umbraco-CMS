import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';

export interface UmbCollectionFilterApi extends UmbApi {
	selection: Observable<Array<string>>;
	setSelection(values: Array<string>): void;
}
