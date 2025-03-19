import type { UmbReferenceByVariantId } from './types.js';
import { UmbPropertyReadOnlyStateManager, type UmbPropertyReadOnlyState } from '@umbraco-cms/backoffice/property';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

export interface UmbVariantPropertyReadOnlyState extends UmbPropertyReadOnlyState {
	propertyType: UmbReferenceByUnique & UmbReferenceByVariantId;
}

export class UmbVariantPropertyReadOnlyStateManager extends UmbPropertyReadOnlyStateManager<UmbVariantPropertyReadOnlyState> {}
