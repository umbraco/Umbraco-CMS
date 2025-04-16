import type { UmbState } from './state.manager.js';
import { UmbReadOnlyStateManager } from './read-only-state.manager.js';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

export interface UmbVariantState extends UmbState {
	variantId: UmbVariantId;
}

export class UmbReadOnlyVariantStateManager extends UmbReadOnlyStateManager<UmbVariantState> {}
