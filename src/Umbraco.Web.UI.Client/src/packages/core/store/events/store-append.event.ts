import { UmbStoreEvent } from './store.event.js';

export class UmbStoreAppendEvent extends UmbStoreEvent {
	static readonly TYPE = 'append';

	public constructor(uniques: Array<string>) {
		super(UmbStoreAppendEvent.TYPE, uniques);
	}
}
