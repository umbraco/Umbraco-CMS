import type { UmbVariantId } from './variant-id.class.js';
import type { UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import type { ScheduleRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbPropertyValueData } from '@umbraco-cms/backoffice/property';

export type UmbObjectWithVariantProperties = {
	culture: string | null;
	segment: string | null;
};

export interface UmbVariantDataModel {
	culture: string | null;
	segment: string | null;
}

export interface UmbVariantPropertyValueModel extends UmbVariantDataModel, UmbPropertyValueData {}

export interface UmbEntityVariantModel {
	name: string;
	culture: string | null;
	segment: string | null;
	createDate: string | null;
	updateDate: string | null;
}

/** @deprecated use `UmbEntityVariantModel` instead */
// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbVariantModel extends UmbEntityVariantModel {}

export interface UmbEntityVariantOptionModel<VariantType extends UmbEntityVariantModel = UmbEntityVariantModel> {
	variant?: VariantType;
	language: UmbLanguageDetailModel;
	/**
	 * The unique identifier is a VariantId string.
	 */
	unique: string;
	culture: string | null;
	segment: string | null;
}

/** @deprecated use `UmbEntityVariantOptionModel` instead */
// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbVariantOptionModel<VariantType extends UmbEntityVariantModel = UmbEntityVariantModel>
	extends UmbEntityVariantOptionModel<VariantType> {}

export interface UmbEntityVariantPublishModel {
	variantId: UmbVariantId;
	schedule?: ScheduleRequestModel | null;
}

export interface UmbReferenceByVariantId {
	variantId: UmbVariantId;
}

/** @deprecated use `UmbEntityVariantPublishModel` instead */
// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbVariantPublishModel extends UmbEntityVariantPublishModel {}
