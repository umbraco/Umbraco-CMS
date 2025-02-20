import type { UmbDocumentBlueprintEntityType } from './entity.js';
import type { UmbEntityVariantModel, UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import { DocumentVariantStateModel as UmbDocumentBlueprintVariantState } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbElementValueModel } from '@umbraco-cms/backoffice/content';

export { UmbDocumentBlueprintVariantState };
export type * from './tree/types.js';
export type * from './workspace/types.js';
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

export interface UmbDocumentBlueprintVariantModel extends UmbEntityVariantModel {
	state?: UmbDocumentBlueprintVariantState | null;
	publishDate?: string | null;
}

export interface UmbDocumentBlueprintUrlInfoModel {
	culture: string | null;
	url: string;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbDocumentBlueprintValueModel<ValueType = unknown> extends UmbElementValueModel<ValueType> {}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbDocumentBlueprintVariantOptionModel
	extends UmbEntityVariantOptionModel<UmbDocumentBlueprintVariantModel> {}
