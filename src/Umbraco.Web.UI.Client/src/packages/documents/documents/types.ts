import type { UmbDocumentEntityType } from './entity.js';
import type { UmbVariantModel } from '@umbraco-cms/backoffice/variant';
import type { ContentUrlInfoModel, DocumentValueModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbDocumentDetailModel {
	documentType: { unique: string };
	entityType: UmbDocumentEntityType;
	isTrashed: boolean;
	template: { id: string } | null; // TODO: change to unique when template is updated
	unique: string;
	parentUnique: string | null;
	urls: Array<ContentUrlInfoModel>;
	values: Array<DocumentValueModel>;
	variants: Array<UmbVariantModel>;
}
