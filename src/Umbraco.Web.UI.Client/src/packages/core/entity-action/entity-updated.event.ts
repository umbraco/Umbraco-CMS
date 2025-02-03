import type { UmbEntityActionEventArgs } from './entity-action.event.js';
import { UmbEntityActionEvent } from './entity-action.event.js';

export class UmbEntityUpdatedEvent extends UmbEntityActionEvent {
	static readonly TYPE = 'entity-updated';

	constructor(args: UmbEntityActionEventArgs) {
		super(UmbEntityUpdatedEvent.TYPE, args);
	}
}
