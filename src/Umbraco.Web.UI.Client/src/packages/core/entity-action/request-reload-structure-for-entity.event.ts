import type { UmbEntityActionEventArgs } from './entity-action.event.js';
import { UmbEntityActionEvent } from './entity-action.event.js';

export class UmbRequestReloadStructureForEntityEvent extends UmbEntityActionEvent {
	static readonly TYPE = 'request-reload-structure-for-entity';

	constructor(args: UmbEntityActionEventArgs) {
		super(UmbRequestReloadStructureForEntityEvent.TYPE, args);
	}
}
