export interface UmbMemberItemModel {
	unique: string;
	memberType: {
		unique: string;
		icon: string;
		hasListView: boolean;
	};
	variants: Array<UmbMemberVariantItemModel>;
}

export interface UmbMemberVariantItemModel {
	name: string;
	culture: string | null;
}
