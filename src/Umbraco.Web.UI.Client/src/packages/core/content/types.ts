import type { UmbPropertyValueData } from '@umbraco-cms/backoffice/property';
import type { UmbEntityVariantModel } from '@umbraco-cms/backoffice/variant';

export interface UmbContentValueModel<ValueType = unknown> extends UmbPropertyValueData<ValueType> {
	editorAlias: string;
	culture: string | null;
	segment: string | null;
}

export interface UmbPotentialContentValueModel<ValueType = unknown> extends UmbPropertyValueData<ValueType> {
	editorAlias?: string;
	culture?: string | null;
	segment?: string | null;
}

export interface UmbContentDetailModel {
	unique: string;
	entityType: string;
	values: Array<UmbContentValueModel>;
	variants: Array<UmbEntityVariantModel>;
}
