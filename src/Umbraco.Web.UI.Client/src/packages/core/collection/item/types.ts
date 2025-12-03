import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
export type * from './entity-collection-item-card/types.js';
export type * from './entity-collection-item-ref/types.js';
export type * from './entity-collection-item-element.interface.js';

export interface UmbCollectionItemModel extends UmbEntityModel {
	unique: string;
	name?: string;
	icon?: string;
}

export interface UmbCollectionItemDetailPropertyConfig {
	alias: string;
	name: string;
	isSystem: boolean;
}
