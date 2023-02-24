import { PackageResource } from '@umbraco-cms/backend-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import type { DataSourceResponse, UmbPackage } from '@umbraco-cms/models';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';
import { umbracoPath } from '@umbraco-cms/utils';

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
	getRootItems(): Promise<DataSourceResponse<UmbPackage[]>> {
		// TODO: Use real resource when available
		return tryExecuteAndNotify(
			this.host,
			fetch(umbracoPath('/manifests')).then((res) => res.json())
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
