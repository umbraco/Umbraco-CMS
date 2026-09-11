import type { UmbEntityActionEventArgs } from './entity-action.event.js';
import { UmbEntityActionEvent } from './entity-action.event.js';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

export interface UmbEntityUpdatedEventArgs extends UmbEntityActionEventArgs {
	/**
	 * The variants that were affected by the update. Empty for entities that do not vary by culture or segment.
	 */
	variantIds?: Array<UmbVariantId>;
}

export class UmbEntityUpdatedEvent extends UmbEntityActionEvent<UmbEntityUpdatedEventArgs> {
	static readonly TYPE = 'entity-updated';

	constructor(args: UmbEntityUpdatedEventArgs) {
		super(UmbEntityUpdatedEvent.TYPE, args);
	}

	/**
	 * Gets the variants that were affected by the update.
	 * @returns {Array<UmbVariantId>} The affected variants, or an empty array for invariant entities.
	 */
	getVariantIds(): Array<UmbVariantId> {
		return this._args.variantIds ?? [];
	}
}
