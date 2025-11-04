import type { UmbEntityModel, UmbNamedEntityModel } from '@umbraco-cms/backoffice/entity';
export type * from './item-data-api-get-request-controller/types.js';
export type * from './data-resolver/types.js';

// TODO: v19 - remove
/**
 * @deprecated - Deprecated since v17. Will be removed in v19. Use UmbItemModel instead.
 */
export interface UmbDefaultItemModel extends UmbNamedEntityModel {
	icon?: string;
}

export interface UmbItemModel extends UmbEntityModel {
	unique: string;
	name?: string;
	icon?: string | null;
}
