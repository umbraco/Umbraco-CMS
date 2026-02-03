import type { UmbElementFolderEntityType } from '../../../entity.js';
import type { UmbEntityFlag, UmbEntityWithFlags } from '@umbraco-cms/backoffice/entity-flag';

export interface UmbElementFolderItemModel extends UmbEntityWithFlags {
	entityType: UmbElementFolderEntityType;
	name: string;
	unique: string;
	icon: string;
	flags: Array<UmbEntityFlag>;
}
