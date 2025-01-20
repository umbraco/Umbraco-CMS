import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbPickerSearchManagerConfig<SearchRequestArgsType = any> {
	providerAlias: string;
	// TODO: searchFrom should have been part of the requestArgs object to make the type more flexible
	searchFrom?: UmbEntityModel;
	requestArgs?: SearchRequestArgsType;
}
