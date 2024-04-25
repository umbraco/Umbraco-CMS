import type { UmbVariantId } from './variant-id.class.js';
import type { UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import type { ScheduleRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbPropertyValueData } from '@umbraco-cms/backoffice/property';

export interface UmbVariantDataModel {
	culture: string | null;
	segment: string | null;
}

export interface UmbVariantPropertyValueModel extends UmbVariantDataModel, UmbPropertyValueData {}

export interface UmbVariantModel {
	name: string;
	culture: string | null;
	segment: string | null;
	createDate: string | null;
	updateDate: string | null;
}

export interface UmbVariantOptionModel<VariantType extends UmbVariantModel = UmbVariantModel> {
	variant?: VariantType;
	language: UmbLanguageDetailModel;
	/**
	 * The unique identifier is a VariantId string.
	 */
	unique: string;
	culture: string | null;
	segment: string | null;
}

export interface UmbVariantPublishModel {
	variantId: UmbVariantId;
	schedule?: ScheduleRequestModel | null;
}
