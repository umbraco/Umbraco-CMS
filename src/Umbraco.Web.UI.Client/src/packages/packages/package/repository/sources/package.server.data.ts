import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { ManifestService, PackageService } from '@umbraco-cms/backoffice/external/backend-api';
import type {
	CreatePackageRequestModel,
	UpdatePackageRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

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
	 * @memberof UmbPackageServerDataSource
	 */
	getRootItems() {
		return tryExecute(this._host, ManifestService.getManifestManifest());
	}

	/**
	 * Get the package configuration from the server
	 * @memberof UmbPackageServerDataSource
	 */
	getPackageConfiguration() {
		return tryExecute(this._host, PackageService.getPackageConfiguration());
	}

	/**
	 * Get the package migrations from the server
	 * @memberof UmbPackageServerDataSource
	 */
	getPackageMigrations() {
		return tryExecute(this._host, PackageService.getPackageMigrationStatus({ query: { skip: 0, take: 9999 } }));
	}

	async saveCreatedPackage(body: CreatePackageRequestModel) {
		return await tryExecute(this._host, PackageService.postPackageCreated({ body }));
	}

	async updateCreatedPackage(id: string, body: UpdatePackageRequestModel) {
		return await tryExecute(this._host, PackageService.putPackageCreatedById({ path: { id }, body }));
	}
}
