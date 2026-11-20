import type { UmbMockDataSet } from '../../../data/mock-data-set.types.js';
import { umbMockServerEvents } from '../../../server-events.js';
import { UmbMockDBBase } from '../mock-db-base.js';

export abstract class UmbCultureMockDbBase<
	MockItemType extends { isoCode: string },
> extends UmbMockDBBase<MockItemType> {
	constructor(dataKey: keyof UmbMockDataSet, data: Array<MockItemType>) {
		super(dataKey, data);
	}

	create(item: MockItemType) {
		this.data.push(item);
		umbMockServerEvents.emit(this.dataKey, 'Created', item.isoCode);
	}

	read(isoCode: string) {
		return this.data.find((item) => item.isoCode === isoCode);
	}

	update(isoCode: string, updatedItem: MockItemType) {
		const itemIndex = this.data.findIndex((item) => item.isoCode === isoCode);
		this.data[itemIndex] = updatedItem;
		umbMockServerEvents.emit(this.dataKey, 'Updated', isoCode);
	}

	delete(isoCode: string) {
		this.data = this.data.filter((item) => item.isoCode !== isoCode);
		umbMockServerEvents.emit(this.dataKey, 'Deleted', isoCode);
	}
}
