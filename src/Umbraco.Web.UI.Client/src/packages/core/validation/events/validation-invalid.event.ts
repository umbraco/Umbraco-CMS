import { UmbValidationEvent } from './validation.event.js';

export class UmbValidationInvalidEvent extends UmbValidationEvent {
	static readonly TYPE = 'invalid';

	public constructor() {
		super(UmbValidationInvalidEvent.TYPE);
	}
}

declare global {
	interface GlobalEventHandlersEventMap {
		[UmbValidationInvalidEvent.TYPE]: UmbValidationInvalidEvent;
	}
}
