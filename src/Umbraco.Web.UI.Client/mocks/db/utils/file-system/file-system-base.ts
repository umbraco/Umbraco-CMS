import type { UmbMockDataSet } from '../../../data/mock-data-set.types.js';
import { umbMockServerEvents } from '../../../server-events.js';
import { UmbMockDBBase } from '../mock-db-base.js';

export abstract class UmbFileSystemMockDbBase<
	MockItemType extends { path: string },
> extends UmbMockDBBase<MockItemType> {
	constructor(dataKey: keyof UmbMockDataSet, data: Array<MockItemType>) {
		super(dataKey, data);
	}

	create(item: MockItemType) {
		this.data.push(item);
		umbMockServerEvents.emit(this.dataKey, 'Created', item.path);
	}

	read(path: string) {
		return this.data.find((item) => item.path === path);
	}

	update(existingPath: string, updatedItem: MockItemType) {
		const itemIndex = this.data.findIndex((item) => item.path === existingPath);
		this.data[itemIndex] = updatedItem;
		umbMockServerEvents.emit(this.dataKey, 'Updated', existingPath);
	}

	delete(path: string) {
		this.data = this.data.filter((item) => item.path !== path);
		umbMockServerEvents.emit(this.dataKey, 'Deleted', path);
	}
}
