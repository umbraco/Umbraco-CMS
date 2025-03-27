import type { UmbMemberEntityType } from '../../entity.js';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

export interface UmbMemberItemModel {
	entityType: UmbMemberEntityType;
	hasChildren: boolean;
	isTrashed: boolean;
	unique: string;
	memberType: {
		unique: string;
		icon: string;
		collection: UmbReferenceByUnique | null;
	};
	name: string; // TODO: get correct variant name
	parent: { unique: UmbEntityUnique } | null; // TODO: Use UmbReferenceByUnique when it support unique as null
	variants: Array<UmbMemberItemVariantModel>;
}

export interface UmbMemberItemVariantModel {
	name: string;
	culture: string | null;
}
