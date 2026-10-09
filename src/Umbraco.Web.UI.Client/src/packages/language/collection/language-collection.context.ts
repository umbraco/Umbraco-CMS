import { UMB_EDIT_LANGUAGE_WORKSPACE_PATH_PATTERN } from '../paths.js';
import type { UmbLanguageDetailModel } from '../types.js';
import type { UmbLanguageCollectionFilterModel } from './types.js';
import { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';

export class UmbLanguageCollectionContext extends UmbDefaultCollectionContext<
	UmbLanguageDetailModel,
	UmbLanguageCollectionFilterModel
> {
	/**
	 * Returns the href for a specific Language collection item.
	 * @param {UmbLanguageDetailModel} item - The language item to get the href for.
	 * @returns {Promise<string | undefined>} - The edit workspace href for the language.
	 */
	override async requestItemHref(item: UmbLanguageDetailModel): Promise<string | undefined> {
		return `${UMB_EDIT_LANGUAGE_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique: item.unique })}`;
	}
}

export { UmbLanguageCollectionContext as api };
