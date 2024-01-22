export interface UmbDocumentItemModel {
	unique: string;
	isTrashed: boolean;
	isProtected: boolean;
	documentType: {
		unique: string;
		icon: string;
		hasListView: boolean;
	};
	variants: Array<UmbDocumentItemVariantModel>;
}

export interface UmbDocumentItemVariantModel {
	name: string;
	culture: string;
	state: string;
}
