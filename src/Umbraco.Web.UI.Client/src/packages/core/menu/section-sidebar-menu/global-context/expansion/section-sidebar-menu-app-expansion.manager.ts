import type { UmbSectionMenuItemExpansionEntryModel } from '../../types.js';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';
import { UmbEntityExpansionManager } from '@umbraco-cms/backoffice/utils';

/**
 * Manages the expansion state of a section sidebar menu.
 * @exports
 * @class UmbSectionSidebarMenuAppExpansionManager
 * @augments {UmbControllerBase}
 */
export class UmbSectionSidebarMenuAppExpansionManager extends UmbEntityExpansionManager<UmbSectionMenuItemExpansionEntryModel> {
	/**
	 * Returns an observable of the expansion state filtered by section alias.
	 * @param {string} sectionAlias The alias of the section to filter by.
	 * @returns {Observable<UmbSectionMenuItemExpansionEntryModel[]>} An observable of the expansion state for the specified section alias.
	 * @memberof UmbSectionSidebarMenuAppExpansionManager
	 */
	expansionBySectionAlias(sectionAlias: string): Observable<UmbSectionMenuItemExpansionEntryModel[]> {
		return this._expansion.asObservablePart((entries) =>
			entries.filter((entry) => entry.sectionAlias === sectionAlias),
		);
	}
}
