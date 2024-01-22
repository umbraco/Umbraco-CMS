import { UmbDocumentEntityType } from './entity.js';
import { ContentStateModel, ContentUrlInfoModel, DocumentValueModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbDocumentDetailModel {
	contentTypeId: string;
	entityType: UmbDocumentEntityType;
	isTrashed: boolean;
	parentUnique: string | null;
	templateId: string | null;
	unique: string;
	// TODO: figure out if we need our own models for these
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
