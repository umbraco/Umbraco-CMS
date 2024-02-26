import type { UmbReferenceById } from '@umbraco-cms/backoffice/models';

export interface UmbMemberItemModel {
	unique: string;
	memberType: {
		unique: string;
		icon: string;
		collection?: UmbReferenceById;
	};
	variants: Array<UmbMemberVariantItemModel>;
}

export interface UmbMemberVariantItemModel {
	name: string;
	culture: string | null;
}
