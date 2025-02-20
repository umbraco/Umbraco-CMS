import { UmbValidationEvent } from './validation.event.js';

export class UmbValidationValidEvent extends UmbValidationEvent {
	static readonly TYPE = 'valid';

	constructor() {
		super(UmbValidationValidEvent.TYPE);
	}
}
