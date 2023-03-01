import { UmbPackageStore, UMB_PACKAGE_STORE_TOKEN } from './package.store';
import { UmbPackageServerDataSource } from './sources/package.server.data';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { ManifestBase } from '@umbraco-cms/extensions-registry';

// TODO: Figure out if we should base stores like this on something more generic for "collections" rather than trees.

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
				this.requestRootItems(instance);
				this.requestPackageMigrations(instance);
				res();
			});
		});
	}

	/**
	 * Request the root items from the Data Source
	 * @memberOf UmbPackageRepository
	 */
	async requestRootItems(store: UmbPackageStore) {
		if (store.isPackagesLoaded) {
			return;
		}

		const { data: packages } = await this.#packageSource.getRootItems();

		if (packages) {
			store.appendItems(packages);
			const extensions: ManifestBase[] = [];

			packages.forEach((p) => {
				p.extensions?.forEach((e) => {
					// Crudely validate that the extension at least follows a basic manifest structure
					// Idea: Use `Zod` to validate the manifest
					if (this.isManifestBase(e)) {
						extensions.push(e);
					}
				});
			});

			store.appendExtensions(extensions);
		}
	}

	/**
	 * Request the package migrations from the Data Source
	 * @memberOf UmbPackageRepository
	 */
	async requestPackageMigrations(store: UmbPackageStore) {
		const { data: migrations } = await this.#packageSource.getPackageMigrations();

		if (migrations) {
			store.appendMigrations(migrations.items);
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

	/**
	 * Observable of extensions
	 * @memberOf UmbPackageRepository
	 */
	async extensions() {
		await this.#init;
		return this.#packageStore!.extensions;
	}

	/**
	 * Observable of migrations
	 * @memberOf UmbPackageRepository
	 */
	async migrations() {
		await this.#init;
		return this.#packageStore!.migrations;
	}

	private isManifestBase(x: unknown): x is ManifestBase {
		return typeof x === 'object' && x !== null && 'alias' in x;
	}
}
