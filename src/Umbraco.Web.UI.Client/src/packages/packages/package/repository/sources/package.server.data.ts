import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { PackageResource } from '@umbraco-cms/backoffice/external/backend-api';
import type {
	CreatePackageRequestModel,
	UpdatePackageRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * Data source for packages from the server
 * @export
 */
export class UmbPackageServerDataSource {
	constructor(private readonly host: UmbControllerHost) {}

	async deleteCreatedPackage(unique: string) {
		return await tryExecuteAndNotify(this.host, PackageResource.deletePackageCreatedById({ id: unique }));
	}

	getCreatedPackage(unique: string) {
		return tryExecuteAndNotify(this.host, PackageResource.getPackageCreatedById({ id: unique }));
	}

	getCreatedPackages({ skip, take }: { skip: number; take: number }) {
		return tryExecuteAndNotify(this.host, PackageResource.getPackageCreated({ skip, take }));
	}

	getCreatePackageDownload(unique: string) {
		return tryExecuteAndNotify(this.host, PackageResource.getPackageCreatedByIdDownload({ id: unique }));
	}

	/**
	 * Get the root items from the server
	 * @memberof UmbPackageServerDataSource
	 */
	getRootItems() {
		return tryExecuteAndNotify(this.host, PackageResource.getPackageManifest());
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

	async saveCreatedPackage(requestBody: CreatePackageRequestModel) {
		return await tryExecuteAndNotify(this.host, PackageResource.postPackageCreated({ requestBody }));
	}

	async updateCreatedPackage(id: string, requestBody: UpdatePackageRequestModel) {
		return await tryExecuteAndNotify(this.host, PackageResource.putPackageCreatedById({ id, requestBody }));
	}
}
