import type { UmbElementVariantOptionModel } from '../types.js';
import type { UmbContentVariantPickerData, UmbContentVariantPickerValue } from '@umbraco-cms/backoffice/content';

export type UmbElementVariantPickerData = UmbContentVariantPickerData<UmbElementVariantOptionModel>;
export type UmbElementVariantPickerValue = UmbContentVariantPickerValue;
