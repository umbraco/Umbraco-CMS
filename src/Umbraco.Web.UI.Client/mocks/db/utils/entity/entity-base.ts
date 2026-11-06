import type { UmbMockDataSet } from '../../../data/mock-data-set.types.js';
import { umbMockServerEvents } from '../../../server-events.js';
import { UmbMockDBBase } from '../mock-db-base.js';

export abstract class UmbEntityMockDbBase<MockItemType extends { id: string }> extends UmbMockDBBase<MockItemType> {
	constructor(dataKey: keyof UmbMockDataSet, data: Array<MockItemType>) {
		super(dataKey, data);
	}

	create(item: MockItemType) {
		this.data.push(item);
		umbMockServerEvents.emit(this.dataKey, 'Created', item.id);
	}

	read(id: string) {
		return this.data.find((item) => item.id === id);
	}

	update(id: string, updatedItem: MockItemType) {
		const itemIndex = this.data.findIndex((item) => item.id === id);
		this.data[itemIndex] = updatedItem;
		umbMockServerEvents.emit(this.dataKey, 'Updated', id);
	}

	delete(id: string) {
		this.data = this.data.filter((item) => item.id !== id);
		umbMockServerEvents.emit(this.dataKey, 'Deleted', id);
	}
}
