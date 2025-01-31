import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbPickerSearchManagerConfig {
	providerAlias: string;
	searchFrom?: UmbEntityModel;
}
