import type { UmbMediaItemModel } from '../../repository/index.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbMediaCardItemModel extends UmbMediaItemModel {
	src?: string;
}

export interface UmbMediaPathModel extends UmbEntityModel {
	name: string;
}
