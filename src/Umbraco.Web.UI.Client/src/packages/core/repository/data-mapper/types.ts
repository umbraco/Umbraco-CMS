import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbDataMapper<fromModelType = any, toModelType = any> extends UmbApi {
	map: (data: fromModelType) => Promise<toModelType>;
}
