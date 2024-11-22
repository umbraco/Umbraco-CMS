import type { UmbUserEntityType } from '../../entity.js';
import type { UmbUserKindType } from '../../utils/index.js';

export interface UmbUserItemModel {
	avatarUrls: Array<string>;
	entityType: UmbUserEntityType;
	kind: UmbUserKindType;
	name: string;
	unique: string;
}
