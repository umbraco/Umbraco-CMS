import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbEntityDataPickerItemModel extends UmbEntityModel {
	unique: string;
	name: string;
	icon: string;
}
