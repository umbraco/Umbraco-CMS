import type { UmbNamedEntityModel } from '@umbraco-cms/backoffice/entity';
export type * from './item-data-api-get-request-controller/types.js';

export interface UmbDefaultItemModel extends UmbNamedEntityModel {
	icon?: string;
}
