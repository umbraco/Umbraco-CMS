export abstract class UmbMockDBBase<T> {
	protected data: Array<T> = [];

	constructor(data: Array<T>) {
		this.data = data;
	}

	getAll() {
		return this.data;
	}

	get total() {
		return this.data.length;
	}

	get(options: { skip: number | undefined; take: number | undefined }) {
		const skip = options.skip ? options.skip : 0;
		const take = options.take ? options.take : 100;

		const mockItems = this.getAll();
		const paginatedItems = mockItems.slice(skip, skip + take);

		return { items: paginatedItems, total: mockItems.length };
	}
}
