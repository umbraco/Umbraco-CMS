import type { UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';

export interface UmbPropertyEditorDataSourceSearchRequestArgs extends UmbSearchRequestArgs {
	dataSourceTypes?: Array<string>;
}
