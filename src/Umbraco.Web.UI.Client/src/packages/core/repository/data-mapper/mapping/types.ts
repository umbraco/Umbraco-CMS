import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
export type * from './data-mapping.extension.js';

export interface UmbDataSourceDataMapping<fromModelType = any, toModelType = any> extends UmbApi {
	map: (data: fromModelType) => Promise<toModelType>;
}
