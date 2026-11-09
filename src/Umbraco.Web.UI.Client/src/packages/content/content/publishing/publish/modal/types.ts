import type { UmbContentVariantPickerData, UmbContentVariantPickerValue } from '../../../variant-picker/types.js';
import type { UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbContentPublishModalData<
	VariantOptionModelType extends UmbEntityVariantOptionModel = UmbEntityVariantOptionModel,
> extends UmbContentVariantPickerData<VariantOptionModelType> {
	headline?: string;
	confirmLabel?: string;
	unique?: string;
	itemRepositoryAlias?: string;
	referenceRepositoryAlias?: string;
	/** Entities directly referenced by this entity's current draft that need attention before publishing. */
	entitiesNeedingAttention?: Array<UmbEntityModel>;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbContentPublishModalValue extends UmbContentVariantPickerValue {}
