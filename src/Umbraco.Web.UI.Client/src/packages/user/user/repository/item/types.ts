import type { UmbUserEntityType } from '../../entity.js';

export interface UmbUserItemModel {
	avatarUrls: Array<string>;
	entityType: UmbUserEntityType;
	name: string;
	unique: string;
}
