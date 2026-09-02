import type { UmbEntityFlag } from '@umbraco-cms/backoffice/entity-flag';
import type { UmbPropertyValueData, UmbPropertyValueDataWithVariant } from '@umbraco-cms/backoffice/property';
import type { UmbEntityVariantModel } from '@umbraco-cms/backoffice/variant';

export type * from './collection/types.js';
export type * from './rollback/types.js';
export type * from './tree/types.js';

export interface UmbEntryDetailModel {
	values: Array<UmbEntryValueModel>;
}
// TODO: Remove in v.21
/**
 * @deprecated Use UmbEntryDetailModel instead
 */
export type UmbElementDetailModel = UmbEntryDetailModel;

export interface UmbEntryValueModel<ValueType = unknown> extends UmbPropertyValueDataWithVariant<ValueType> {
	editorAlias: string;
}

// TODO: Remove in v.21
/**
 * @deprecated Use UmbEntryValueModel instead
 */
export type UmbElementValueModel<ValueType = unknown> = UmbEntryValueModel<ValueType>;

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbContentValueModel<ValueType = unknown> extends UmbEntryValueModel<ValueType> {}

export interface UmbPotentialContentValueModel<ValueType = unknown> extends UmbPropertyValueData<ValueType> {
	editorAlias?: string;
	culture?: string | null;
	segment?: string | null;
}

export interface UmbEntryWithVariantsDetailModel<
	VariantModelType extends UmbEntityVariantModel = UmbEntityVariantModel,
> extends UmbEntryDetailModel {
	entityType: string;
	variants: Array<VariantModelType>;
}

export interface UmbContentDetailModel<
	VariantModelType extends UmbEntityVariantModel = UmbEntityVariantModel,
> extends UmbEntryDetailModel {
	unique: string;
	entityType: string;
	variants: Array<VariantModelType>;
	flags: Array<UmbEntityFlag>;
}

export interface UmbContentLikeDetailModel
	extends UmbEntryDetailModel, Partial<Pick<UmbContentDetailModel, 'variants' | 'flags'>> {}
