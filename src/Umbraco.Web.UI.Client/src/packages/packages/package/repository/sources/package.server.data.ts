import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { ManifestService, PackageService } from '@umbraco-cms/backoffice/external/backend-api';
import type {
	CreatePackageRequestModel,
	ManifestResponseModel,
	PackageConfigurationResponseModel,
	PagedPackageMigrationStatusResponseModel,
	UpdatePackageRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

/**
 * Data source for packages from the server
 
 */
export class UmbPackageServerDataSource {
	constructor(private readonly _host: UmbControllerHost) {}

	async deleteCreatedPackage(unique: string) {
		return await tryExecute(this._host, PackageService.deletePackageCreatedById({ path: { id: unique } }));
	}

	getCreatedPackage(unique: string) {
		return tryExecute(this._host, PackageService.getPackageCreatedById({ path: { id: unique } }));
	}

	getCreatedPackages({ skip, take }: { skip: number; take: number }) {
		return tryExecute(this._host, PackageService.getPackageCreated({ query: { skip, take } }));
	}

	getCreatePackageDownload(unique: string) {
		return tryExecute(this._host, PackageService.getPackageCreatedByIdDownload({ path: { id: unique } }));
	}

	/**
	 * Get the root items from the server
	 * @returns {Promise<UmbDataSourceResponse<Array<ManifestResponseModel>>>} The root items.
	 * @memberof UmbPackageServerDataSource
	 */
	getRootItems(): Promise<UmbDataSourceResponse<Array<ManifestResponseModel>>> {
		return tryExecute(this._host, ManifestService.getManifestManifest());
	}

	/**
	 * Get the package configuration from the server
	 * @returns {Promise<UmbDataSourceResponse<PackageConfigurationResponseModel>>} The package configuration.
	 * @memberof UmbPackageServerDataSource
	 */
	getPackageConfiguration(): Promise<UmbDataSourceResponse<PackageConfigurationResponseModel>> {
		return tryExecute(this._host, PackageService.getPackageConfiguration());
	}

	/**
	 * Get the package migrations from the server
	 * @returns {Promise<UmbDataSourceResponse<PagedPackageMigrationStatusResponseModel>>} The package migrations.
	 * @memberof UmbPackageServerDataSource
	 */
	getPackageMigrations(): Promise<UmbDataSourceResponse<PagedPackageMigrationStatusResponseModel>> {
		return tryExecute(this._host, PackageService.getPackageMigrationStatus({ query: { skip: 0, take: 9999 } }));
	}

	async saveCreatedPackage(body: CreatePackageRequestModel) {
		return await tryExecute(this._host, PackageService.postPackageCreated({ body }));
	}

	async updateCreatedPackage(id: string, body: UpdatePackageRequestModel) {
		return await tryExecute(this._host, PackageService.putPackageCreatedById({ path: { id }, body }));
	}
}
