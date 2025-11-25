import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
export type * from './entity-collection-item-card/types.js';

export interface UmbCollectionItemModel extends UmbEntityModel {
	unique: string;
	name?: string;
	icon?: string;
}
