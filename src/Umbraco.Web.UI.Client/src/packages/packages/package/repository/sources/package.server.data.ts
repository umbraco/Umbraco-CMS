import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { ManifestResource, PackageResource } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * Data source for packages from the server
 * @export
 */
export class UmbPackageServerDataSource {
	constructor(private readonly host: UmbControllerHost) {}

	/**
	 * Get the root items from the server
	 * @memberof UmbPackageServerDataSource
	 */
	getRootItems() {
		return tryExecuteAndNotify(this.host, ManifestResource.getManifestManifest());
	}

	/**
	 * Get the package configuration from the server
	 * @memberof UmbPackageServerDataSource
	 */
	getPackageConfiguration() {
		return tryExecuteAndNotify(this.host, PackageResource.getPackageConfiguration());
	}

	/**
	 * Get the package migrations from the server
	 * @memberof UmbPackageServerDataSource
	 */
	getPackageMigrations() {
		return tryExecuteAndNotify(this.host, PackageResource.getPackageMigrationStatus({ skip: 0, take: 9999 }));
	}
}
