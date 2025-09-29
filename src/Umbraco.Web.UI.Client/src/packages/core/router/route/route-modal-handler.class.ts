import type { UmbModalRouteHandler } from '@umbraco-cms/backoffice/modal';

/**
 * Implementation of UmbModalRouteHandler that delegates to a route context
 */
export class UmbRouteModalHandler implements UmbModalRouteHandler {
	readonly #removeModalPath: (path?: string) => void;

	constructor(removeModalPath: (path?: string) => void) {
		this.#removeModalPath = removeModalPath;
	}

	removeModalPath(path?: string): void {
		this.#removeModalPath(path);
	}
}
