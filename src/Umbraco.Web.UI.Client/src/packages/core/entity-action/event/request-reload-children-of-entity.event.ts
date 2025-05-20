import type { UmbEntityActionEventArgs } from './entity-action.event.js';
import { UmbEntityActionEvent } from './entity-action.event.js';

export class UmbRequestReloadChildrenOfEntityEvent extends UmbEntityActionEvent {
	static readonly TYPE = 'request-reload-children-of-entity';

	constructor(args: UmbEntityActionEventArgs) {
		super(UmbRequestReloadChildrenOfEntityEvent.TYPE, args);
	}
}
