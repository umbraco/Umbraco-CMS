import type { UmbMockDataSet } from './data/types/mock-data-set.types.js';
import { umbMockDbRegistry } from './db/mock-db-registry.js';

/**
 * Central manager for mock data sets.
 * Handles loading mock sets and coordinating DB initialization.
 */
class UmbMockManager {
	#currentSetName: string = 'default';
	#currentDataSet: UmbMockDataSet | null = null;
	#initialized: boolean = false;

	// Lazy loaders for mock sets
	#mockSetLoaders: Record<string, () => Promise<UmbMockDataSet>> = {
		default: () => import('./data/sets/default/index.js') as Promise<UmbMockDataSet>,
		kenn: () => import('./data/sets/kenn/index.js') as Promise<UmbMockDataSet>,
		test: () => import('./data/sets/test/index.js') as Promise<UmbMockDataSet>,
	};

	/**
	 * Get current mock set name.
	 */
	get currentSetName(): string {
		return this.#currentSetName;
	}

	/**
	 * Get current data set. Throws if not initialized.
	 */
	getDataSet(): UmbMockDataSet {
		if (!this.#currentDataSet) {
			throw new Error('MockManager not initialized. Call initialize() first.');
		}
		return this.#currentDataSet;
	}

	/**
	 * Initialize with a specific mock set.
	 * This loads the data set and populates all registered DBs.
	 */
	async initialize(setName: string = 'default'): Promise<void> {
		await this.#loadSet(setName);
		this.#initialized = true;

		// Load data into all registered DBs
		umbMockDbRegistry.loadAll(this.#currentDataSet!);

		console.log(`[MSW] Using mock data set: "${this.#currentSetName}"`);
	}

	/**
	 * Switch to a different mock set at runtime.
	 * Loads the new set and updates all DBs with the new data.
	 */
	async useMockSet(setName: string): Promise<void> {
		await this.#loadSet(setName);

		// Load data into all registered DBs
		umbMockDbRegistry.loadAll(this.#currentDataSet!);

		console.log(`[MSW] Switched to mock data set: "${this.#currentSetName}"`);
	}

	/**
	 * Clear all DBs (empty them).
	 */
	clearAll(): void {
		umbMockDbRegistry.clearAll();
	}

	/**
	 * Load a mock set by name.
	 */
	async #loadSet(setName: string): Promise<void> {
		const loader = this.#mockSetLoaders[setName];
		if (!loader) {
			console.warn(`Mock set "${setName}" not found, falling back to "default"`);
			this.#currentDataSet = await this.#mockSetLoaders['default']();
			this.#currentSetName = 'default';
		} else {
			this.#currentDataSet = await loader();
			this.#currentSetName = setName;
		}
	}
}

// Singleton instance
export const umbMockManager = new UmbMockManager();

// Convenience functions for tests
export async function useMockSet(setName: string): Promise<void> {
	return umbMockManager.useMockSet(setName);
}

export function clearMockData(): void {
	umbMockManager.clearAll();
}
