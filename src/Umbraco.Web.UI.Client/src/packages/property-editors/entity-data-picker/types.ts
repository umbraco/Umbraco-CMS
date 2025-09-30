import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbEntityDataItemModel extends UmbEntityModel {
	unique: string;
	name: string;
	icon: string;
}

export interface UmbEntityDataTreeItemModel extends UmbEntityDataItemModel {
	parent: UmbEntityModel;
	hasChildren: boolean;
}
