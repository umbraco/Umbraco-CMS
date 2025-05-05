import type { UmbNamedEntityModel } from '@umbraco-cms/backoffice/entity';
export type * from './data-api-item-get-request-controller/types.js';

export interface UmbDefaultItemModel extends UmbNamedEntityModel {
	icon?: string;
}
