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
	getRootItems(): Promise<DataSourceResponse<{ items: UmbPackage[] }>> {
		// TODO: Use real resource when available
		return tryExecuteAndNotify(
			this.host,
			fetch(umbracoPath('/manifests')).then((res) => res.json())
		);
	}
}
