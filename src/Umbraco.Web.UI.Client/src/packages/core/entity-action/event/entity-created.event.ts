import type { UmbEntityActionEventArgs } from './entity-action.event.js';
import { UmbEntityActionEvent } from './entity-action.event.js';

export class UmbEntityCreatedEvent extends UmbEntityActionEvent {
	static readonly TYPE = 'entity-created';

	constructor(args: UmbEntityActionEventArgs) {
		super(UmbEntityCreatedEvent.TYPE, args);
	}
}
