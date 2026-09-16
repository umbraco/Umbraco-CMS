import type { UmbDocumentBlueprintEntityType } from './entity.js';
import type { UmbDocumentBlueprintVariantState } from './variant-state.js';
import type { UmbEntityVariantModel, UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbContentDetailModel, UmbEntryValueModel } from '@umbraco-cms/backoffice/content';

export type * from './tree/types.js';
export type * from './workspace/types.js';
export interface UmbDocumentBlueprintDetailModel extends UmbContentDetailModel {
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
export interface UmbDocumentBlueprintValueModel<ValueType = unknown> extends UmbEntryValueModel<ValueType> {}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbDocumentBlueprintVariantOptionModel extends UmbEntityVariantOptionModel<UmbDocumentBlueprintVariantModel> {}
