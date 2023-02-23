import { UmbPackageStore, UMB_PACKAGE_STORE_TOKEN } from './package.store';
import { UmbPackageServerDataSource } from './sources/package.server.data';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';

/**
 * A repository for Packages which mimicks a tree store.
 * @export
 */
export class UmbPackageRepository {
	#init!: Promise<void>;
	#packageStore?: UmbPackageStore;
	#packageSource: UmbPackageServerDataSource;

	constructor(host: UmbControllerHostInterface) {
		this.#packageSource = new UmbPackageServerDataSource(host);
		this.#init = new Promise((res) => {
			new UmbContextConsumerController(host, UMB_PACKAGE_STORE_TOKEN, (instance) => {
				this.#packageStore = instance;
				this.#requestRootItems(instance);
				res();
			});
		});
	}

	/**
	 * Request the root items from the Data Source
	 * @memberOf UmbPackageRepository
	 * @private
	 */
	async #requestRootItems(store: UmbPackageStore) {
		const { data } = await this.#packageSource.getRootItems();

		if (data) {
			store.appendItems(data.items);
		}
	}

	/**
	 * Observable of root items
	 * @memberOf UmbPackageRepository
	 */
	async rootItems() {
		await this.#init;
		return this.#packageStore!.rootItems;
	}
}
