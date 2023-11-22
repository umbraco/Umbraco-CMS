import { UmbStoreEvent } from './store.event.js';

export class UmbStoreDeleteEvent extends UmbStoreEvent {
	static readonly TYPE = 'delete';

	public constructor(uniques: Array<string>) {
		super(UmbStoreDeleteEvent.TYPE, uniques);
	}
}
