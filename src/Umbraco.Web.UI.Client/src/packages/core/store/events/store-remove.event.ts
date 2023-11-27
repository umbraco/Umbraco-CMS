import { UmbStoreEvent } from './store.event.js';

export class UmbStoreRemoveEvent extends UmbStoreEvent {
	static readonly TYPE = 'remove';

	public constructor(uniques: Array<string>) {
		super(UmbStoreRemoveEvent.TYPE, uniques);
	}
}
