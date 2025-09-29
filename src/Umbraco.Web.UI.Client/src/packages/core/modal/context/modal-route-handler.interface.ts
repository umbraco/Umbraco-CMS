/**
 * Interface for handling modal route operations.
 * This abstraction allows modal contexts to interact with routing
 * without directly depending on router implementations.
 */
export interface UmbModalRouteHandler {
	/**
	 * Removes a modal path from the current route
	 * @param path - The modal path to remove
	 */
	removeModalPath(path?: string): void;
}
