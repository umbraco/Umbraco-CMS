import { UmbStoreEvent } from './store.event.js';

export class UmbStoreCreateEvent extends UmbStoreEvent {
	static readonly TYPE = 'create';

	public constructor(uniques: Array<string>) {
		super(UmbStoreCreateEvent.TYPE, uniques);
	}
}
