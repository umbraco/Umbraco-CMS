import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
export type * from './item-data-api-get-request-controller/types.js';
export type * from './data-resolver/types.js';

export interface UmbItemModel extends UmbEntityModel {
	unique: string;
	name?: string;
	icon?: string | null;
}
