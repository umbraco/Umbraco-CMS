import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
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
	constructor(private readonly host: UmbControllerHost) {}

	async deleteCreatedPackage(unique: string) {
		return await tryExecuteAndNotify(this.host, PackageService.deletePackageCreatedById({ id: unique }));
	}

	getCreatedPackage(unique: string) {
		return tryExecuteAndNotify(this.host, PackageService.getPackageCreatedById({ id: unique }));
	}

	getCreatedPackages({ skip, take }: { skip: number; take: number }) {
		return tryExecuteAndNotify(this.host, PackageService.getPackageCreated({ skip, take }));
	}

	getCreatePackageDownload(unique: string) {
		return tryExecuteAndNotify(this.host, PackageService.getPackageCreatedByIdDownload({ id: unique }));
	}

	/**
	 * Get the root items from the server
	 * @memberof UmbPackageServerDataSource
	 */
	getRootItems() {
		return tryExecuteAndNotify(this.host, ManifestService.getManifestManifest());
	}

	/**
	 * Get the package configuration from the server
	 * @memberof UmbPackageServerDataSource
	 */
	getPackageConfiguration() {
		return tryExecuteAndNotify(this.host, PackageService.getPackageConfiguration());
	}

	/**
	 * Get the package migrations from the server
	 * @memberof UmbPackageServerDataSource
	 */
	getPackageMigrations() {
		return tryExecuteAndNotify(this.host, PackageService.getPackageMigrationStatus({ skip: 0, take: 9999 }));
	}

	async saveCreatedPackage(requestBody: CreatePackageRequestModel) {
		return await tryExecuteAndNotify(this.host, PackageService.postPackageCreated({ requestBody }));
	}

	async updateCreatedPackage(id: string, requestBody: UpdatePackageRequestModel) {
		return await tryExecuteAndNotify(this.host, PackageService.putPackageCreatedById({ id, requestBody }));
	}
}
