import type { UmbEntityMockDbBase } from './entity-base.js';

export class UmbMockEntityDetailManager<MockType extends { id: string }> {
	#db: UmbEntityMockDbBase<MockType>;
	#createMockItemMapper: (request: any) => MockType;
	#readResponseMapper: (mockItem: MockType) => any;

	constructor(
		db: UmbEntityMockDbBase<MockType>,
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
		return mockItem.id;
	}

	read(id: string): MockType {
		const item = this.#db.read(id);
		if (!item) throw new Error('Item not found');
		const mappedItem = this.#readResponseMapper(item);
		return mappedItem;
	}

	list(options: { skip?: number; take?: number } = {}): { items: Array<MockType>; total: number } {
		const skip = options.skip ?? 0;
		const take = options.take ?? 100;
		const all = this.#db.getAll();
		const items = all.slice(skip, skip + take).map((item) => this.#readResponseMapper(item));
		return { items, total: all.length };
	}

	readBatch(ids: Array<string>): { items: Array<MockType>; total: number } {
		const items = ids
			.map((id) => {
				try {
					return this.read(id);
				} catch {
					return undefined;
				}
			})
			.filter((item): item is MockType => item !== undefined);
		return { items, total: items.length };
	}

	update(id: string, item: any) {
		const mockItem = this.#db.read(id);

		const updatedMockItem = {
			...mockItem,
			...item,
		} as unknown as MockType;

		this.#db.update(id, updatedMockItem);
	}

	delete(id: string) {
		this.#db.delete(id);
	}

	// TODO: Implement actual validation logic. Currently always returns valid.
	validate(_id?: string) {}
}
