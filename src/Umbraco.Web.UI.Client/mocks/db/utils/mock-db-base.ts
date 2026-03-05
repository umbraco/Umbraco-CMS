import type { UmbMockDataSet } from '../../data/types/mock-data-set.types.js';
import { umbMockDbRegistry } from '../mock-db-registry.js';

export abstract class UmbMockDBBase<T> {
	protected data: Array<T> = [];

	/**
	 * @param dataKey - The key in UmbMockDataSet this DB corresponds to (used for auto-registration)
	 * @param data - Initial data array
	 */
	constructor(dataKey: keyof UmbMockDataSet, data: Array<T>) {
		this.data = structuredClone(data);
		// Auto-register with the registry
		umbMockDbRegistry.register(dataKey, this);
	}

	/**
	 * Replaces DB data with new data (used when switching mock sets).
	 * @param data
	 */
	setData(data: Array<T>) {
		this.data = structuredClone(data);
	}

	/**
	 * Empties the DB completely.
	 */
	clear() {
		this.data = [];
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
