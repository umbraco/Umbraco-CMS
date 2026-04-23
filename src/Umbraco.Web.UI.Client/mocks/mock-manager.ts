import type { UmbMockDataSet } from './data/mock-data-set.types.js';
import { umbMockDbRegistry } from './db/mock-db-registry.js';

interface UmbMockSetEntry {
	label: string;
	loader: () => Promise<UmbMockDataSet>;
	/** Whether the set appears in the header app dropdown. Defaults to false. */
	visible?: boolean;
}

/**
 * Central manager for mock data sets.
 * Handles loading mock sets and coordinating DB initialization.
 */
class UmbMockManager {
	#currentSetName: string = 'default';
	#currentDataSet: UmbMockDataSet | null = null;

	// Lazy loaders for mock sets
	#mockSetLoaders: Record<string, UmbMockSetEntry> = {
		default: {
			label: 'Default',
			loader: () => import('./data/sets/default/index.js') as Promise<UmbMockDataSet>,
			visible: true,
		},
		kitchenSink: {
			label: 'Kitchen Sink',
			loader: () => import('./data/sets/kitchen-sink/index.js') as Promise<UmbMockDataSet>,
			visible: true,
		},
		userPermissions: {
			label: 'User Permissions',
			loader: () => import('./data/sets/user-permissions/index.js') as Promise<UmbMockDataSet>,
			visible: false,
		},
	};

	/**
	 * Get current mock set name.
	 */
	get currentSetName(): string {
		return this.#currentSetName;
	}

	/**
	 * Get the label of the current mock set.
	 */
	get currentSetLabel(): string {
		return this.#mockSetLoaders[this.#currentSetName]?.label ?? this.#currentSetName;
	}

	/**
	 * Get all visible mock sets as alias/label pairs.
	 * Only sets with `visible: true` are included.
	 */
	get availableSets(): Array<{ alias: string; label: string }> {
		return Object.entries(this.#mockSetLoaders)
			.filter(([, { visible }]) => visible === true)
			.map(([alias, { label }]) => ({ alias, label }));
	}

	/**
	 * Get all available mock set names.
	 */
	get availableSetNames(): Array<string> {
		return Object.keys(this.#mockSetLoaders);
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
	 * @param setName
	 */
	async initialize(setName: string = 'default'): Promise<void> {
		await this.#loadSet(setName);

		// Load data into all registered DBs
		umbMockDbRegistry.loadAll(this.#currentDataSet!);

		console.log(`[MSW] Using mock data set: "${this.#currentSetName}"`);
	}

	/**
	 * Switch to a different mock set at runtime.
	 * Loads the new set and updates all DBs with the new data.
	 * @param setName
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
	 * @param setName
	 */
	async #loadSet(setName: string): Promise<void> {
		const entry = this.#mockSetLoaders[setName];
		if (!entry) {
			console.warn(`Mock set "${setName}" not found, falling back to "default"`);
			this.#currentDataSet = await this.#mockSetLoaders['default'].loader();
			this.#currentSetName = 'default';
		} else {
			this.#currentDataSet = await entry.loader();
			this.#currentSetName = setName;
		}
	}
}

// Singleton instance
export const umbMockManager = new UmbMockManager();

// Convenience functions for tests
/**
 *
 * @param setName
 */
export async function useMockSet(setName: string): Promise<void> {
	return umbMockManager.useMockSet(setName);
}

/**
 *
 */
export function clearMockData(): void {
	umbMockManager.clearAll();
}
