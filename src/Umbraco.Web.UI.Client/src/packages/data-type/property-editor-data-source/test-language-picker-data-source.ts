import type { UmbPickerPropertyEditorCollectionDataSource } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbCollectionFilterModel } from '@umbraco-cms/backoffice/collection';
import { UmbLanguageCollectionRepository, UmbLanguageItemRepository } from '@umbraco-cms/backoffice/language';

export class UmbLanguagePickerPropertyEditorDataSource
	extends UmbControllerBase
	implements UmbPickerPropertyEditorCollectionDataSource
{
	#collection = new UmbLanguageCollectionRepository(this);
	#item = new UmbLanguageItemRepository(this);
	#config: any;

	setConfig(config: any) {
		this.#config = config;
	}

	getConfig(): any {
		return this.#config;
	}

	requestCollection(args: UmbCollectionFilterModel) {
		return this.#collection.requestCollection(args);
	}

	requestItems(uniques: Array<string>) {
		return this.#item.requestItems(uniques);
	}
}

export { UmbLanguagePickerPropertyEditorDataSource as api };
