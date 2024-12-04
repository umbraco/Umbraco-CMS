import type { UmbDocumentVariantOptionModel } from '../types.js';
import type { UmbContentVariantPickerData, UmbContentVariantPickerValue } from '@umbraco-cms/backoffice/content';

export type UmbDocumentVariantPickerData = UmbContentVariantPickerData<UmbDocumentVariantOptionModel>;
export type UmbDocumentVariantPickerValue = UmbContentVariantPickerValue;

export type * from './publish-modal/constants.js';
export type * from './publish-with-descendants-modal/constants.js';
export type * from './save-modal/constants.js';
export type * from './unpublish-modal/constants.js';
export type * from './schedule-modal/constants.js';
export type * from './document-picker-modal.token.js';
