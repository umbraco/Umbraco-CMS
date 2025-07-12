import type { UmbEntityActionEventArgs } from './entity-action.event.js';
import { UmbEntityActionEvent } from './entity-action.event.js';

interface UmbGenericEntityActionEventArgs extends UmbEntityActionEventArgs {
	type: string;
}

export class UmbEntityEvent extends UmbEntityActionEvent {
	constructor(args: UmbGenericEntityActionEventArgs) {
		super(args.type, args);
	}
}
