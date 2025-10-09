import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbCollectionItemModel extends UmbEntityModel {
	unique: string;
	name?: string;
	icon?: string;
}
