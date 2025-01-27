import type { UmbEntityActionEventArgs } from './entity-action.event.js';
import { UmbEntityActionEvent } from './entity-action.event.js';

export class UmbEntityDetailUpdatedEvent extends UmbEntityActionEvent {
	static readonly TYPE = 'entity-detail-updated';

	constructor(args: UmbEntityActionEventArgs) {
		super(UmbEntityDetailUpdatedEvent.TYPE, args);
	}
}
