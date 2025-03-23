import type { UmbReferenceByVariantId } from './types.js';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import { UmbPropertyViewStateManager, type UmbPropertyViewState } from '@umbraco-cms/backoffice/property';

export interface UmbVariantPropertyViewState extends UmbPropertyViewState {
	propertyType: UmbReferenceByUnique & UmbReferenceByVariantId;
}

export class UmbVariantPropertyViewStateManager extends UmbPropertyViewStateManager<UmbVariantPropertyViewState> {}
