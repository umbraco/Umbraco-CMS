import type { UmbStructureItemModel, UmbStructureItemModelBase } from './types.js';
import type { UmbContext } from '@umbraco-cms/backoffice/class-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

/**
 * Base context for exposing an entity's ancestor/parent structure to menu UI.
 * @template TStructureItemModel - The shape of each item in the structure.
 */
export interface UmbMenuStructureWorkspaceContext<
	TStructureItemModel extends UmbStructureItemModelBase = UmbStructureItemModel,
> extends UmbContext {
	/**
	 * Observable array of structure items representing the workspace's breadcrumb hierarchy.
	 */
	structure: Observable<Array<TStructureItemModel>>;

	/**
	 * Returns the href for a breadcrumb structure item, or `undefined` if the item should not be a link.
	 * Used by the workspace breadcrumb element to generate clickable navigation links.
	 * @param {TStructureItemModel} structureItem The structure item to generate an href for.
	 * @returns {string | undefined} The href string, or `undefined` if the item should not be clickable.
	 */
	getItemHref?(structureItem: TStructureItemModel): string | undefined;
}
