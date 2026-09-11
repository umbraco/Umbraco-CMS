import type { UmbEntityActionEventArgs } from './entity-action.event.js';
import { UmbEntityActionEvent } from './entity-action.event.js';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

export interface UmbEntityCreatedEventArgs extends UmbEntityActionEventArgs {
	/**
	 * The variants that were affected by the creation. Empty for entities that do not vary by culture or segment.
	 */
	variantIds?: Array<UmbVariantId>;
}

export class UmbEntityCreatedEvent extends UmbEntityActionEvent<UmbEntityCreatedEventArgs> {
	static readonly TYPE = 'entity-created';

	constructor(args: UmbEntityCreatedEventArgs) {
		super(UmbEntityCreatedEvent.TYPE, args);
	}

	/**
	 * Gets the variants that were affected by the creation.
	 * @returns {Array<UmbVariantId>} The affected variants, or an empty array for invariant entities.
	 */
	getVariantIds(): Array<UmbVariantId> {
		return this._args.variantIds ?? [];
	}
}
