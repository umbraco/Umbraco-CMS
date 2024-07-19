import type { UmbDocumentBlueprintEntityType } from './entity.js';
import type { UmbVariantModel, UmbVariantOptionModel } from '@umbraco-cms/backoffice/variant';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import { DocumentVariantStateModel as UmbDocumentBlueprintVariantState } from '@umbraco-cms/backoffice/external/backend-api';
export { UmbDocumentBlueprintVariantState };

export interface UmbDocumentBlueprintDetailModel {
	documentType: {
		unique: string;
		collection: UmbReferenceByUnique | null;
	};
	entityType: UmbDocumentBlueprintEntityType;
	unique: string;
	values: Array<UmbDocumentBlueprintValueModel>;
	variants: Array<UmbDocumentBlueprintVariantModel>;
}

export interface UmbDocumentBlueprintVariantModel extends UmbVariantModel {
	state?: UmbDocumentBlueprintVariantState | null;
	publishDate?: string | null;
}

export interface UmbDocumentBlueprintUrlInfoModel {
	culture: string | null;
	url: string;
}

export interface UmbDocumentBlueprintValueModel<ValueType = unknown> {
	culture: string | null;
	segment: string | null;
	alias: string;
	value: ValueType;
}

export interface UmbDocumentBlueprintVariantOptionModel
	extends UmbVariantOptionModel<UmbDocumentBlueprintVariantModel> {}
