import type { UmbMediaItemModel } from '../../repository/index.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbMediaCardItemModel extends UmbMediaItemModel {
	url?: string;
}

export interface UmbMediaPathModel extends UmbEntityModel {
	name: string;
}
