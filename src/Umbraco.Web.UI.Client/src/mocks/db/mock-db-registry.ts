import type { UmbMockDataSet } from '../data/types/mock-data-set.types.js';

interface MockDbEntry {
	setData: (data: any) => void;
	clear: () => void;
}

/**
 * Central registry for mock DBs.
 * DBs auto-register themselves when instantiated via UmbMockDBBase.
 */
class UmbMockDbRegistry {
	#dbs = new Map<keyof UmbMockDataSet, MockDbEntry>();

	/**
	 * Register a DB with its corresponding data key.
	 * Called automatically by UmbMockDBBase constructor.
	 */
	register(dataKey: keyof UmbMockDataSet, db: MockDbEntry) {
		this.#dbs.set(dataKey, db);
	}

	/**
	 * Load data from a mock data set into all registered DBs.
	 */
	loadAll(dataSet: UmbMockDataSet) {
		for (const [key, db] of this.#dbs) {
			const data = dataSet[key];
			db.setData(data ?? []);
		}
	}

	/**
	 * Clear all registered DBs.
	 */
	clearAll() {
		for (const db of this.#dbs.values()) {
			db.clear();
		}
	}

	/**
	 * Get count of registered DBs (useful for debugging).
	 */
	get size() {
		return this.#dbs.size;
	}
}

export const umbMockDbRegistry = new UmbMockDbRegistry();
