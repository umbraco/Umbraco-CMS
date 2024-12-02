import type { UmbCultureMockDbBase } from './culture-base.js';

export class UmbMockCultureDetailManager<MockType extends { isoCode: string }> {
	#db: UmbCultureMockDbBase<MockType>;
	#createMockItemMapper: (request: any) => MockType;
	#readResponseMapper: (mockItem: MockType) => any;

	constructor(
		db: UmbCultureMockDbBase<MockType>,
		createMockItemMapper: (request: any) => MockType,
		readResponseMapper: (mockItem: MockType) => any,
	) {
		this.#db = db;
		this.#createMockItemMapper = createMockItemMapper;
		this.#readResponseMapper = readResponseMapper;
	}

	create(request: any) {
		const mockItem = this.#createMockItemMapper(request);
		// create mock item in mock db
		this.#db.create(mockItem);
		return mockItem.isoCode;
	}

	read(id: string) {
		const item = this.#db.read(id);
		if (!item) throw new Error('Item not found');
		const mappedItem = this.#readResponseMapper(item);
		return mappedItem;
	}

	update(isoCode: string, item: any) {
		const mockItem = this.#db.read(isoCode);

		const updatedMockItem = {
			...mockItem,
			...item,
		} as unknown as MockType;

		this.#db.update(isoCode, updatedMockItem);
	}

	delete(isoCode: string) {
		this.#db.delete(isoCode);
	}
}
