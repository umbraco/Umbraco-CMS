import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbCollectionItemModel extends UmbEntityModel {
	name?: string;
	icon?: string;
}
