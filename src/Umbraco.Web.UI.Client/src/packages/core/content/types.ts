import type { UmbPropertyValueData } from '@umbraco-cms/backoffice/property';
import type { UmbEntityVariantModel } from '@umbraco-cms/backoffice/variant';

export interface UmbElementDetailModel {
	values: Array<UmbElementValueModel>;
}

export interface UmbElementValueModel<ValueType = unknown> extends UmbPropertyValueData<ValueType> {
	editorAlias: string;
	culture: string | null;
	segment: string | null;
}
// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbContentValueModel<ValueType = unknown> extends UmbElementValueModel<ValueType> {}

export interface UmbPotentialContentValueModel<ValueType = unknown> extends UmbPropertyValueData<ValueType> {
	editorAlias?: string;
	culture?: string | null;
	segment?: string | null;
}

export interface UmbContentDetailModel extends UmbElementDetailModel {
	unique: string;
	entityType: string;
	variants: Array<UmbEntityVariantModel>;
}

export interface UmbContentLikeDetailModel
	extends UmbElementDetailModel,
		Partial<Pick<UmbContentDetailModel, 'variants'>> {}
