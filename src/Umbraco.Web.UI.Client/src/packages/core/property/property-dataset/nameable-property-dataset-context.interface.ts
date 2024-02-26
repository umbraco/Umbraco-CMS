import type { UmbPropertyDatasetContext } from './property-dataset-context.interface.js';

/**
 * A variant context with ability to set the name of it.
 */
export interface UmbNameablePropertyDatasetContext extends UmbPropertyDatasetContext {
	setName(name: string): void;
}
