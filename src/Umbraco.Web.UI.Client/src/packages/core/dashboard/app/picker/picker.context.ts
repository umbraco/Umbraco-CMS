import type { UmbDashboardAppDetailModel } from '../types.js';
import { UmbDashboardAppCollectionRepository } from '../collection.repository.js';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbCollectionItemPickerContext } from '@umbraco-cms/backoffice/collection';

export class UmbDashboardAppPickerContext extends UmbCollectionItemPickerContext {
	public getUnique = (item: UmbDashboardAppDetailModel) => item.unique;

	#items = new UmbArrayState<UmbDashboardAppDetailModel>([], (x) => x.unique);
	public items = this.#items.asObservable();

	#take = 100;
	#collectionRepository = new UmbDashboardAppCollectionRepository(this);

	async loadData() {
		const { data, error } = await this.#collectionRepository.requestCollection({
			take: this.#take,
		});

		if (error) {
			this.#items.setValue([]);
		}

		if (data) {
			this.#items.setValue(data.items);
		}
	}
}
