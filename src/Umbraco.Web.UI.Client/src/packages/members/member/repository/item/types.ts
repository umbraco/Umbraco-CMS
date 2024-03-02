import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

export interface UmbMemberItemModel {
	unique: string;
	memberType: {
		unique: string;
		icon: string;
		collection: UmbReferenceByUnique | null;
	};
	variants: Array<UmbMemberVariantItemModel>;
}

export interface UmbMemberVariantItemModel {
	name: string;
	culture: string | null;
}
