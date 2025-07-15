import { UmbMockDBBase } from '../mock-db-base.js';

export abstract class UmbCultureMockDbBase<
	MockItemType extends { isoCode: string },
> extends UmbMockDBBase<MockItemType> {
	constructor(data: Array<MockItemType>) {
		super(data);
	}

	create(item: MockItemType) {
		this.data.push(item);
	}

	read(isoCode: string) {
		return this.data.find((item) => item.isoCode === isoCode);
	}

	update(isoCode: string, updatedItem: MockItemType) {
		const itemIndex = this.data.findIndex((item) => item.isoCode === isoCode);
		this.data[itemIndex] = updatedItem;
	}

	delete(isoCode: string) {
		this.data = this.data.filter((item) => item.isoCode !== isoCode);
	}
}
