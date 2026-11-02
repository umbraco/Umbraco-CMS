import type { UmbContentVariantPickerData, UmbContentVariantPickerValue } from '../../../variant-picker/types.js';
import type { UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';

export interface UmbContentPublishModalData<
	VariantOptionModelType extends UmbEntityVariantOptionModel = UmbEntityVariantOptionModel,
> extends UmbContentVariantPickerData<VariantOptionModelType> {
	headline?: string;
	confirmLabel?: string;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbContentPublishModalValue extends UmbContentVariantPickerValue {}
