import { PackageResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * Data source for packages from the server
 * @export
 */
export class UmbPackageServerDataSource {
	constructor(private readonly host: UmbControllerHostElement) {}

	/**
	 * Get the root items from the server
	 * @memberof UmbPackageServerDataSource
	 */
	getRootItems() {
		return tryExecuteAndNotify(this.host, PackageResource.getPackageManifest());
	}

	/**
	 * Get the package migrations from the server
	 * @memberof UmbPackageServerDataSource
	 */
	getPackageMigrations() {
		return tryExecuteAndNotify(this.host, PackageResource.getPackageMigrationStatus({ skip: 0, take: 9999 }));
	}
}
