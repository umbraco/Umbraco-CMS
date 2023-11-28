import { UmbPackageStore, UMB_PACKAGE_STORE_TOKEN } from './package.store.js';
import { UmbPackageServerDataSource } from './sources/package.server.data.js';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import { UmbApi, ManifestBase, isManifestBaseType } from '@umbraco-cms/backoffice/extension-api';
import { OpenAPI } from '@umbraco-cms/backoffice/backend-api';

/**
 * A repository for Packages which mimics a tree store.
 * @export
 */
export class UmbPackageRepository extends UmbBaseController implements UmbApi {
	#init!: Promise<void>;
	#packageStore?: UmbPackageStore;
	#packageSource: UmbPackageServerDataSource;
	#apiBaseUrl = OpenAPI.BASE;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#packageSource = new UmbPackageServerDataSource(this);
		this.#init = new Promise((res) => {
			this.consumeContext(UMB_PACKAGE_STORE_TOKEN, (instance) => {
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
			// Append packages to the store but only if they have a name
			store.appendItems(packages.filter((p) => p.name?.length));
			const extensions: ManifestBase[] = [];

			packages.forEach((p) => {
				p.extensions?.forEach((e) => {
					// Crudely validate that the extension at least follows a basic manifest structure
					// Idea: Use `Zod` to validate the manifest
					if (isManifestBaseType(e)) {
						/**
						 * Crude check to see if extension is of type "js" since it is safe to assume we do not
						 * need to load any other types of extensions in the backoffice (we need a js file to load)
						 */
						// Add base url if the js path is relative
						if ('js' in e && typeof e.js === 'string' && !e.js.startsWith('http')) {
							e.js = `${this.#apiBaseUrl}${e.js}`;
						}

						// Add base url if the element path is relative
						if ('element' in e && typeof e.element === 'string' && !e.element.startsWith('http')) {
							e.element = `${this.#apiBaseUrl}${e.element}`;
						}

						// Add base url if the element path api relative
						if ('api' in e && typeof e.api === 'string' && !e.api.startsWith('http')) {
							e.api = `${this.#apiBaseUrl}${e.api}`;
						}

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
}
