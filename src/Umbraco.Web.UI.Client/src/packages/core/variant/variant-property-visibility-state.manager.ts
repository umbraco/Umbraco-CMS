import type { UmbReferenceByVariantId } from './types.js';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import { UmbPropertyVisibilityStateManager, type UmbPropertyVisibilityState } from '@umbraco-cms/backoffice/property';

export interface UmbVariantPropertyVisibilityState extends UmbPropertyVisibilityState {
	propertyType: UmbReferenceByUnique & UmbReferenceByVariantId;
}

export class UmbVariantPropertyVisibilityStateManager extends UmbPropertyVisibilityStateManager<UmbVariantPropertyVisibilityState> {}
