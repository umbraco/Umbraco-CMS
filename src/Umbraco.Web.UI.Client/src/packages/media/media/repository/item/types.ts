import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

export interface UmbMediaItemModel {
	unique: string;
	isTrashed: boolean;
	mediaType: {
		unique: string;
		icon: string;
		collection: UmbReferenceByUnique | null;
	};
	variants: Array<UmbMediaItemVariantModel>;
	name: string; // TODO: get correct variant name
}

export interface UmbMediaItemVariantModel {
	name: string;
	culture: string | null;
}
