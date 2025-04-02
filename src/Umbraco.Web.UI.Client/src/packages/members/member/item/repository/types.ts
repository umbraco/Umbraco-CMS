import type { UmbMemberEntityType } from '../../entity.js';
import type { UmbMemberKindType } from '../../utils/index.js';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

export interface UmbMemberItemModel {
	entityType: UmbMemberEntityType;
	unique: string;
	name: string; // TODO: this is not correct. We need to get it from the variants. This is a temp solution.
	memberType: {
		unique: string;
		icon: string;
		collection: UmbReferenceByUnique | null;
	};
	variants: Array<UmbMemberItemVariantModel>;
	kind: UmbMemberKindType;
}

export interface UmbMemberItemVariantModel {
	name: string;
	culture: string | null;
}

/**
 * @deprecated Deprecated in favor of UmbMemberItemVariantModel. Will be removed in v17.0.0
 * @interface UmbMemberVariantItemModel
 */
export interface UmbMemberVariantItemModel {
	name: string;
	culture: string | null;
}
