export interface UmbMediaItemModel {
	unique: string;
	isTrashed: boolean;
	mediaType: {
		unique: string;
		icon: string;
		hasListView: boolean;
	};
	variants: Array<UmbMediaItemVariantModel>;
	name: string; // TODO: get correct variant name
}

export interface UmbMediaItemVariantModel {
	name: string;
	culture: string | null;
}
