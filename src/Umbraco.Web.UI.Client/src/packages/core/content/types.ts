import type { UmbPropertyValueData } from '@umbraco-cms/backoffice/property';
import type { UmbEntityVariantModel } from '@umbraco-cms/backoffice/variant';

export type * from './collection/types.js';

export interface UmbElementDetailModel {
	values: Array<UmbElementValueModel>;
}

export interface UmbElementValueModel<ValueType = unknown> extends UmbPropertyValueData<ValueType> {
	culture: string | null;
	editorAlias: string;
	entityType: string;
	segment: string | null;
}
// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbContentValueModel<ValueType = unknown> extends UmbElementValueModel<ValueType> {}

export interface UmbPotentialContentValueModel<ValueType = unknown> extends UmbPropertyValueData<ValueType> {
	editorAlias?: string;
	culture?: string | null;
	segment?: string | null;
}

export interface UmbContentDetailModel<VariantModelType extends UmbEntityVariantModel = UmbEntityVariantModel>
	extends UmbElementDetailModel {
	unique: string;
	entityType: string;
	variants: Array<VariantModelType>;
}

export interface UmbContentLikeDetailModel
	extends UmbElementDetailModel,
		Partial<Pick<UmbContentDetailModel, 'variants'>> {}
