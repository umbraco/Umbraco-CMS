import type { UmbEntityActionEventArgs } from './entity-action.event.js';
import { UmbEntityActionEvent } from './entity-action.event.js';

export class UmbEntityDeletedEvent extends UmbEntityActionEvent {
	static readonly TYPE = 'entity-deleted';

	constructor(args: UmbEntityActionEventArgs) {
		super(UmbEntityDeletedEvent.TYPE, args);
	}
}
