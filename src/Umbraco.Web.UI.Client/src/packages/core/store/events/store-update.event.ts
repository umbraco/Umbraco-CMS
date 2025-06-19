import { UmbStoreEvent } from './store.event.js';

export class UmbStoreUpdateEvent extends UmbStoreEvent {
	static readonly TYPE = 'update';

	public constructor(uniques: Array<string>) {
		super(UmbStoreUpdateEvent.TYPE, uniques);
	}
}
