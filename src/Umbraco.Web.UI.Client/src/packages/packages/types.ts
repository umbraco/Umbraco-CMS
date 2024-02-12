import type { PackageManifestResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export type UmbPackage = PackageManifestResponseModel;

export type PackageManifestResponse = UmbPackage[];

export type UmbPackageWithMigrationStatus = UmbPackage & {
	hasPendingMigrations: boolean;
};
