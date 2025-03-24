import { UmbPropertyViewStateManager, type UmbPropertyViewState } from './property-view-state.manager.js';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbReferenceByVariantId } from '@umbraco-cms/backoffice/variant';

export interface UmbVariantPropertyViewState extends UmbPropertyViewState {
	propertyType: UmbReferenceByUnique & UmbReferenceByVariantId;
}

export class UmbVariantPropertyViewStateManager extends UmbPropertyViewStateManager<UmbVariantPropertyViewState> {}
