import { UmbPropertyReadOnlyStateManager, type UmbPropertyReadOnlyState } from './property-read-only-state.manager.js';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbReferenceByVariantId } from '@umbraco-cms/backoffice/variant';

export interface UmbVariantPropertyReadOnlyState extends UmbPropertyReadOnlyState {
	propertyType: UmbReferenceByUnique & UmbReferenceByVariantId;
}

export class UmbVariantPropertyReadOnlyStateManager extends UmbPropertyReadOnlyStateManager<UmbVariantPropertyReadOnlyState> {}
