import { UmbPropertyWriteStateManager, type UmbPropertyWriteState } from './property-write-state.manager.js';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbReferenceByVariantId } from '@umbraco-cms/backoffice/variant';

export interface UmbVariantPropertyWriteState extends UmbPropertyWriteState {
	propertyType: UmbReferenceByUnique & UmbReferenceByVariantId;
}

export class UmbVariantPropertyWriteStateManager extends UmbPropertyWriteStateManager<UmbVariantPropertyWriteState> {}
