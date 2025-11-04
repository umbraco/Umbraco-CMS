import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';

export interface UmbUserGroupItemModel extends UmbItemModel {
	name: string;
	icon: string | null;
}
