import { ReplaySubject } from 'rxjs';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbStoreBase } from '@umbraco-cms/store';
import type { ManifestBase, UmbPackage } from '@umbraco-cms/models';
import type { PackageMigrationStatusModel } from '@umbraco-cms/backend-api';
import { ArrayState } from '@umbraco-cms/observable-api';

/**
 * Store for Packages
 * @export
 * @extends {UmbStoreBase}
 */
export class UmbPackageStore extends UmbStoreBase {
	/**
	 * Array of packages with extensions
	 * @private
	 */
	#packages = new ReplaySubject<Array<UmbPackage>>(1);

	#extensions = new ArrayState<ManifestBase>([], (e) => e.alias);

	#migrations = new ArrayState<PackageMigrationStatusModel>([], (e) => e.packageName);

	/**
	 * Observable of packages with extensions
	 */
	rootItems = this.#packages.asObservable();

	extensions = this.#extensions.asObservable();

	migrations = this.#migrations.asObservable();

	isPackagesLoaded = false;

	/**
	 * Creates an instance of PackageStore.
	 * @param {UmbControllerHostInterface} host
	 * @memberof PackageStore
	 */
	constructor(host: UmbControllerHostInterface) {
		super(host, UmbPackageStore.name);
	}

	/**
	 * Append items to the store
	 */
	appendItems(packages: Array<UmbPackage>) {
		this.#packages.next(packages);
		this.isPackagesLoaded = true;
	}

	appendExtensions(extensions: ManifestBase[]) {
		this.#extensions.append(extensions);
	}

	appendMigrations(migrations: PackageMigrationStatusModel[]) {
		this.#migrations.append(migrations);
	}
}

export const UMB_PACKAGE_STORE_TOKEN = new UmbContextToken<UmbPackageStore>(UmbPackageStore.name);
