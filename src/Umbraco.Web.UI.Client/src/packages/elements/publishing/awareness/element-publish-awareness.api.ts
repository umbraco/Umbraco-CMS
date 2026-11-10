import type { UmbElementItemModel } from '../../types.js';
import { UmbElementVariantState } from '../../variant-state.js';
import type { UmbEntityPublishAwarenessApi } from '@umbraco-cms/backoffice/content';

// Lower rank = worse state; a variant-aware element needs attention if its *worst* variant is Draft, Trashed or PublishedPendingChanges.
const STATE_RANK: Record<string, number> = {
	[UmbElementVariantState.DRAFT]: 0,
	[UmbElementVariantState.TRASHED]: 1,
	[UmbElementVariantState.PUBLISHED_PENDING_CHANGES]: 2,
	[UmbElementVariantState.PUBLISHED]: 3,
	[UmbElementVariantState.NOT_CREATED]: 4,
};

export class UmbElementPublishAwarenessApi implements UmbEntityPublishAwarenessApi<UmbElementItemModel> {
	needsAttention(item: UmbElementItemModel): boolean {
		const state = this.#getAggregateState(item);
		return (
			state === UmbElementVariantState.DRAFT ||
			state === UmbElementVariantState.TRASHED ||
			state === UmbElementVariantState.PUBLISHED_PENDING_CHANGES
		);
	}

	#getAggregateState(item: UmbElementItemModel): UmbElementVariantState | null | undefined {
		let worst: UmbElementVariantState | null | undefined;
		for (const variant of item.variants) {
			if (worst === undefined || this.#rank(variant.state) < this.#rank(worst)) {
				worst = variant.state;
			}
		}
		return worst;
	}

	#rank(state: UmbElementVariantState | null | undefined): number {
		if (state == null) return STATE_RANK[UmbElementVariantState.NOT_CREATED];
		return STATE_RANK[state] ?? STATE_RANK[UmbElementVariantState.NOT_CREATED];
	}

	destroy(): void {}
}
