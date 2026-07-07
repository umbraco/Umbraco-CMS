import type { UmbContentVariantPickerData, UmbContentVariantPickerValue } from '../../../variant-picker/types.js';
import type { UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';

export interface UmbContentUnpublishModalData<
	VariantOptionModelType extends UmbEntityVariantOptionModel = UmbEntityVariantOptionModel,
> extends UmbContentVariantPickerData<VariantOptionModelType> {
	unique?: string;
	itemRepositoryAlias?: string;
	referenceRepositoryAlias?: string;
	configurationRepositoryAlias?: string;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbContentUnpublishModalValue extends UmbContentVariantPickerValue {}
