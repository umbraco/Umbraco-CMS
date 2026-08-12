import type { UmbStructureItemModel } from './types.js';
import type { UmbContext } from '@umbraco-cms/backoffice/class-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbMenuStructureWorkspaceContext extends UmbContext {
	/**
	 * Observable array of structure items representing the workspace's breadcrumb hierarchy.
	 */
	structure: Observable<UmbStructureItemModel[]>;

	/**
	 * Returns the href for a breadcrumb structure item, or `undefined` if the item should not be a link.
	 * Used by the workspace breadcrumb element to generate clickable navigation links.
	 *
	 * @param structureItem The structure item to generate an href for.
	 * @returns The href string, or `undefined` if the item should not be clickable.
	 */
	getItemHref(structureItem: UmbStructureItemModel): string | undefined;
}
