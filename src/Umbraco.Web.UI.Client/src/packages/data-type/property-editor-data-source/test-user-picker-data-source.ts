import type { UmbPickerPropertyEditorCollectionDataSource } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbUserCollectionRepository, UmbUserItemRepository } from '@umbraco-cms/backoffice/user';

export class UmbUserPickerPropertyEditorDataSource
	extends UmbControllerBase
	implements UmbPickerPropertyEditorCollectionDataSource
{
	#collection = new UmbUserCollectionRepository(this);
	#item = new UmbUserItemRepository(this);
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

export { UmbUserPickerPropertyEditorDataSource as api };
