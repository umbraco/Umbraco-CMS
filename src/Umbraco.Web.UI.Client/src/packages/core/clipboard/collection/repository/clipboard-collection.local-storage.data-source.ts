import type { UmbClipboardEntry } from '../../types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbClipboardCollectionFilterModel } from '../types.js';

export class UmbClipboardCollectionLocalStorageDataSource implements UmbCollectionDataSource<UmbClipboardEntry> {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	async getCollection(filter: UmbClipboardCollectionFilterModel) {
		debugger;
	}
}
