import { ReplaySubject } from 'rxjs';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbPackage } from '@umbraco-cms/backoffice/models';
import type { PackageMigrationStatusResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { ManifestBase } from '@umbraco-cms/backoffice/extensions-registry';
import { ArrayState } from '@umbraco-cms/backoffice/observable-api';

export const UMB_PACKAGE_STORE_TOKEN = new UmbContextToken<UmbPackageStore>('UmbPackageStore');

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

	#migrations = new ArrayState<PackageMigrationStatusResponseModel>([], (e) => e.packageName);

	/**
	 * Observable of packages with extensions
	 */
	rootItems = this.#packages.asObservable();

	extensions = this.#extensions.asObservable();

	migrations = this.#migrations.asObservable();

	isPackagesLoaded = false;

	/**
	 * Creates an instance of PackageStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof PackageStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_PACKAGE_STORE_TOKEN.toString());
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

	appendMigrations(migrations: PackageMigrationStatusResponseModel[]) {
		this.#migrations.append(migrations);
	}
}
