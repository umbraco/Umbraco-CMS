import type { UmbPickerPropertyEditorCollectionDataSource } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbLanguageCollectionRepository, UmbLanguageItemRepository } from '@umbraco-cms/backoffice/language';

export class UmbLanguagePickerPropertyEditorDataSource
	extends UmbControllerBase
	implements UmbPickerPropertyEditorCollectionDataSource
{
	#collection = new UmbLanguageCollectionRepository(this);
	#item = new UmbLanguageItemRepository(this);
	#config: any;

	setConfig(config: any): void {
		this.#config = config;
	}

	getConfig(): any {
		return this.#config;
	}

	requestCollection(args: any): Promise<any> {
		return this.#collection.requestCollection({ skip: args.skip, take: args.take });
	}

	requestItems(uniques: Array<string>): Promise<any> {
		return this.#item.requestItems(uniques);
	}
}

export { UmbLanguagePickerPropertyEditorDataSource as api };
