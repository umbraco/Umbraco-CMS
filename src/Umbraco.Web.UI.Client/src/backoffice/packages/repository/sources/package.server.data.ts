import { PackageResource } from '@umbraco-cms/backend-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';

/**
 * Data source for packages from the server
 * @export
 */
export class UmbPackageServerDataSource {
	constructor(private readonly host: UmbControllerHostInterface) {}

	/**
	 * Get the root items from the server
	 * @memberof UmbPackageServerDataSource
	 */
	getRootItems() {
		return tryExecuteAndNotify(
			this.host,
			PackageResource.getPackageManifest()
		);
	}

	/**
	 * Get the package migrations from the server
	 * @memberof UmbPackageServerDataSource
	 */
	getPackageMigrations() {
		return tryExecuteAndNotify(this.host, PackageResource.getPackageMigrationStatus({ skip: 0, take: 9999 }));
	}
}
