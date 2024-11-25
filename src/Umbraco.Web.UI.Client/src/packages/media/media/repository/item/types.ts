import type { UmbMediaEntityType } from '../../entity.js';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

export interface UmbMediaItemModel {
	entityType: UmbMediaEntityType;
	hasChildren: boolean;
	isTrashed: boolean;
	unique: string;
	mediaType: {
		unique: string;
		icon: string;
		collection: UmbReferenceByUnique | null;
	};
	name: string; // TODO: get correct variant name
	parent: { unique: UmbEntityUnique } | null; // TODO: Use UmbReferenceByUnique when it support unique as null
	variants: Array<UmbMediaItemVariantModel>;
}

export interface UmbMediaItemVariantModel {
	name: string;
	culture: string | null;
}
