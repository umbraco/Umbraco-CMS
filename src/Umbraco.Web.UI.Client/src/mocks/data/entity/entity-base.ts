import { UmbData } from '../data.js';

export abstract class UmbEntityMockDbBase<MockItemType extends { id: string }> extends UmbData<MockItemType> {
	constructor(data: Array<MockItemType>) {
		super(data);
	}

	create(item: MockItemType) {
		this.data.push(item);
	}

	read(id: string) {
		return this.data.find((item) => item.id === id);
	}

	update(id: string, updatedItem: MockItemType) {
		const itemIndex = this.data.findIndex((item) => item.id === id);
		this.data[itemIndex] = updatedItem;
	}

	delete(id: string) {
		this.data = this.data.filter((item) => item.id !== id);
	}

	get(options: { skip: number | undefined; take: number | undefined }) {
		const skip = options.skip ? options.skip : 0;
		const take = options.take ? options.take : 100;

		const mockItems = this.data;
		const paginatedItems = mockItems.slice(skip, skip + take);

		return { items: paginatedItems, total: mockItems.length };
	}
}
