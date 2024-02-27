import type { UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';

export interface UmbVariantModel {
	createDate: string | null;
	culture: string | null;
	name: string;
	segment: string | null;
	updateDate: string | null;
}

export interface UmbVariantOptionModel<VariantType extends UmbVariantModel = UmbVariantModel> {
	variant?: VariantType;
	language: UmbLanguageDetailModel;
	/**
	 * The unique identifier is a VariantId string.
	 */
	unique: string;
}
