import { ROUTER_SLOT_TAG_NAME } from '../config.js';
import type { IRouterSlot } from '../model.js';

/**
 * Queries the parent router.
 * @param $elem
 */
export function queryParentRouterSlot<D = any>($elem: Element): IRouterSlot<D> | null {
	return queryParentRoots<IRouterSlot<D>>($elem, ROUTER_SLOT_TAG_NAME);
}

/**
 * Traverses the roots and returns the first match.
 * The minRoots parameter indicates how many roots should be traversed before we started matching with the query.
 * @param $elem
 * @param query
 * @param minRoots
 * @param roots
 */
export function queryParentRoots<T>($elem: Element, query: string, minRoots: number = 0, roots: number = 0): T | null {
	// Grab the rood node and query it
	const $root = (<any>$elem).getRootNode();

	// If we are at the right level or above we can query!
	if (roots >= minRoots) {
		// See if there's a match
		const match = $root.querySelector(query);
		if (match != null && match != $elem) {
			return match;
		}
	}

	// If a parent root with a host doesn't exist we don't continue the traversal
	const $rootRootNode = $root.getRootNode();
	if ($rootRootNode.host == null) {
		return null;
	}

	// We continue the traversal if there was not matches
	return queryParentRoots($rootRootNode.host, query, minRoots, ++roots);
}
