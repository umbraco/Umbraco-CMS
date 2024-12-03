import type { UmbCreatedPackageDefinition, UmbCreatedPackages } from '../../types.js';
import { UMB_PACKAGE_STORE_TOKEN } from './package.store.context-token.js';
import { UmbPackageServerDataSource } from './sources/package.server.data.js';
import type { UmbPackageStore } from './package.store.js';
import { isManifestBaseType } from '@umbraco-cms/backoffice/extension-api';
import { OpenAPI } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi, ManifestBase } from '@umbraco-cms/backoffice/extension-api';

/**
 * A repository for Packages which mimics a tree store.
 
 */
export class UmbPackageRepository extends UmbControllerBase implements UmbApi {
	#init!: Promise<void>;
	#packageStore?: UmbPackageStore;
	#packageSource: UmbPackageServerDataSource;
	#apiBaseUrl = OpenAPI.BASE;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#packageSource = new UmbPackageServerDataSource(this);
		this.#init = new Promise((resolve) => {
			this.consumeContext(UMB_PACKAGE_STORE_TOKEN, (instance) => {
				this.#packageStore = instance;
				this.requestConfiguration(instance);
				this.requestRootItems(instance);
				this.requestPackageMigrations(instance);
				resolve();
			});
		});
	}

	async getCreatedPackage(unique: string | undefined): Promise<UmbCreatedPackageDefinition> {
		if (!unique) {
			return this.#getEmptyCreatedPackage();
		}

		const { data } = await this.#packageSource.getCreatedPackage(unique);

		if (!data) {
			return this.#getEmptyCreatedPackage();
		}

		const { id, ...model } = data;
		return { unique: id, ...model };
	}

	async getCreatedPackages({ skip, take }: { skip: number; take: number }): Promise<UmbCreatedPackages | undefined> {
		const { data } = await this.#packageSource.getCreatedPackages({ skip, take });
		if (!data) return undefined;
		return {
			items: data.items?.map((item) => ({ unique: item.id, name: item.name })),
			total: data.total,
		};
	}

	async getCreatePackageDownload(unique: string): Promise<Blob | undefined> {
		const { data } = await this.#packageSource.getCreatePackageDownload(unique);
		return data;
	}

	#getEmptyCreatedPackage(): UmbCreatedPackageDefinition {
		return {
			unique: '',
			name: '',
			packagePath: '',
			contentNodeId: undefined,
			contentLoadChildNodes: false,
			mediaIds: [],
			mediaLoadChildNodes: false,
			documentTypes: [],
			mediaTypes: [],
			dataTypes: [],
			templates: [],
			partialViews: [],
			stylesheets: [],
			scripts: [],
			languages: [],
			dictionaryItems: [],
		};
	}

	async deleteCreatedPackage(unique: string) {
		const { error } = await this.#packageSource.deleteCreatedPackage(unique);
		return !error;
	}

	async requestConfiguration(store: UmbPackageStore) {
		const { data } = await this.#packageSource.getPackageConfiguration();
		if (data) {
			store.setConfiguration(data);
		}
	}

	/**
	 * Request the root items from the Data Source
	 * @param store
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
	 * @param store
	 * @memberOf UmbPackageRepository
	 */
	async requestPackageMigrations(store: UmbPackageStore) {
		const { data: migrations } = await this.#packageSource.getPackageMigrations();

		if (migrations) {
			store.appendMigrations(migrations.items);
		}
	}

	async saveCreatedPackage(pkg: UmbCreatedPackageDefinition) {
		// eslint-disable-next-line @typescript-eslint/no-unused-vars
		const { unique: _, ...model } = pkg;
		const { data } = await this.#packageSource.saveCreatedPackage(model);
		return data;
	}

	async updateCreatedPackage(pkg: UmbCreatedPackageDefinition): Promise<boolean> {
		const { unique, ...model } = pkg;
		const { error } = await this.#packageSource.updateCreatedPackage(unique, model);
		return !error;
	}

	async configuration() {
		await this.#init;
		return this.#packageStore!.configuration;
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

export default UmbPackageRepository;
