import type { UmbDocumentEntityType } from './entity.js';
import type { UmbVariantModel, UmbVariantOptionModel, UmbVariantPublishModel } from '@umbraco-cms/backoffice/variant';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import { DocumentVariantStateModel as UmbDocumentVariantState } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbContentValueModel } from '@umbraco-cms/backoffice/content';
export { UmbDocumentVariantState };

export interface UmbDocumentDetailModel {
	documentType: {
		unique: string;
		collection: UmbReferenceByUnique | null;
	};
	entityType: UmbDocumentEntityType;
	isTrashed: boolean;
	template: { unique: string } | null;
	unique: string;
	urls: Array<UmbDocumentUrlInfoModel>;
	values: Array<UmbDocumentValueModel>;
	variants: Array<UmbDocumentVariantModel>;
}

export interface UmbDocumentVariantModel extends UmbVariantModel {
	state: UmbDocumentVariantState | null;
	publishDate: string | null;
}

export interface UmbDocumentUrlInfoModel {
	culture: string | null;
	url: string;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbDocumentValueModel<ValueType = unknown> extends UmbContentValueModel<ValueType> {}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbDocumentVariantOptionModel extends UmbVariantOptionModel<UmbDocumentVariantModel> {}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbDocumentVariantPublishModel extends UmbVariantPublishModel {}
