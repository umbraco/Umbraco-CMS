import type { UmbMediaEntityType } from '../../entity.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbMediaCardItemModel {
	name: string;
	unique: string;
	entityType: UmbMediaEntityType;
	url?: string;
	icon?: string;
}

export interface UmbMediaPathModel extends UmbEntityModel {
	name: string;
}
