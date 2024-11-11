import type { UmbState } from './state.manager.js';
import { UmbReadOnlyStateManager } from './read-only-state.manager.js';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbVariantState extends UmbState {
	variantId: UmbVariantId;
}

export class UmbReadOnlyVariantStateManager extends UmbReadOnlyStateManager<UmbVariantState> {
	constructor(host: UmbControllerHost) {
		super(host);
	}
}
