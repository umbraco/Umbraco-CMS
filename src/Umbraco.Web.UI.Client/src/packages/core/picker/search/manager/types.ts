import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbPickerSearchManagerConfig<QueryParamsType = Record<string, unknown>> {
	providerAlias: string;
	searchFrom?: UmbEntityModel;
	queryParams?: QueryParamsType;
}
