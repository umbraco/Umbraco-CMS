import type { UmbVariantId } from './variant-id.class.js';
// TODO: Remove import of language module. Core can not depend on a package
import type { UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import type { ScheduleRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbPropertyValueData } from '@umbraco-cms/backoffice/property';
import type { UmbEntityFlag } from '@umbraco-cms/backoffice/entity-flag';

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
	// TODO: Can we remove partial from this one: [NL]
	state?: string | null;
	flags: Array<UmbEntityFlag>;
}

/** @deprecated use `UmbEntityVariantModel` instead */
// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbVariantModel extends UmbEntityVariantModel {}

export interface UmbEntityVariantOptionModel<VariantType extends UmbEntityVariantModel = UmbEntityVariantModel> {
	variant?: VariantType;
	language: UmbLanguageDetailModel;
	segmentInfo?: {
		alias: string;
		name: string;
		cultures?: string[] | null;
	};
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

/** @deprecated use `UmbEntityVariantPublishModel` instead */
// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbVariantPublishModel extends UmbEntityVariantPublishModel {}
