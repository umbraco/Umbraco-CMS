import type { UmbElementEntityType } from '../../entity.js';
import type { UmbElementVariantState } from '../../types.js';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import type { UmbEntityFlag, UmbEntityWithFlags } from '@umbraco-cms/backoffice/entity-flag';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

export interface UmbElementItemModel extends UmbEntityWithFlags {
	documentType: {
		unique: string;
		icon: string;
		collection?: UmbReferenceByUnique | null;
	};
	entityType: UmbElementEntityType;
	hasChildren: boolean;
	isTrashed: boolean;
	parent: { unique: UmbEntityUnique } | null; // TODO: Use UmbReferenceByUnique when it support unique as null
	unique: string;
	variants: Array<UmbElementItemVariantModel>;
}

export interface UmbElementItemVariantModel {
	name: string;
	culture: string | null;
	createDate?: Date;
	updateDate?: Date;
	state?: UmbElementVariantState | null;
	flags: Array<UmbEntityFlag>;
}
