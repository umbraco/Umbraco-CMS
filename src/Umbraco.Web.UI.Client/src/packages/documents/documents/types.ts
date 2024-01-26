import type { UmbDocumentEntityType } from './entity.js';
import type { ContentStateModel, ContentUrlInfoModel, DocumentValueModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbDocumentDetailModel {
	documentType: { unique: string };
	entityType: UmbDocumentEntityType;
	isTrashed: boolean;
	template: { id: string } | null; // TODO: change to unique when template is updated
	unique: string;
	urls: Array<ContentUrlInfoModel>;
	values: Array<DocumentValueModel>;
	variants: Array<UmbDocumentVariantModel>;
}

export interface UmbDocumentVariantModel {
	createDate: string | null;
	culture: string | null;
	name: string;
	publishDate: string | null;
	segment: string | null;
	state: ContentStateModel | null;
	updateDate: string | null;
}
