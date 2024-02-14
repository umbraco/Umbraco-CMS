import type { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';

export interface UmbDocumentItemModel {
	name: string; // TODO: this is not correct. We need to get it from the variants. This is a temp solution.
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
	culture: string | null;
	state: DocumentVariantStateModel | null;
}
