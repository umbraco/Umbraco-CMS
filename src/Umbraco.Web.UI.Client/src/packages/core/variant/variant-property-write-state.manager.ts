import type { UmbReferenceByVariantId } from './types.js';
import { UmbPropertyWriteStateManager, type UmbPropertyWriteState } from '@umbraco-cms/backoffice/property';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

export interface UmbVariantPropertyWriteState extends UmbPropertyWriteState {
	propertyType: UmbReferenceByUnique & UmbReferenceByVariantId;
}

export class UmbVariantPropertyWriteStateManager extends UmbPropertyWriteStateManager<UmbVariantPropertyWriteState> {}
