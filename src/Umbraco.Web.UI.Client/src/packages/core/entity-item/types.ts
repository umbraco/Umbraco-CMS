import type { UmbEntityModel, UmbNamedEntityModel } from '@umbraco-cms/backoffice/entity';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbEntityItemModel extends UmbEntityModel {}

export interface UmbDefaultItemModel extends UmbNamedEntityModel {
	icon?: string;
}
