import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbPropertyReadOnlyStateManager, type UmbPropertyReadOnlyState } from './property-read-only-state.manager.js';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

export interface UmbVariantPropertyReadOnlyState extends UmbPropertyReadOnlyState {
	propertyType: UmbReferenceByUnique;
	variantId: UmbVariantId;
}

export class UmbVariantPropertyReadOnlyStateManager extends UmbPropertyReadOnlyStateManager<UmbVariantPropertyReadOnlyState> {}
