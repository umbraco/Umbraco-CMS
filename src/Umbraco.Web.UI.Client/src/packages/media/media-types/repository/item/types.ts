import type { UmbMediaTypeEntityType } from '../../entity.js';

export interface UmbMediaTypeItemModel {
	entityType: UmbMediaTypeEntityType;
	icon: string | null;
	name: string;
	unique: string;
}
