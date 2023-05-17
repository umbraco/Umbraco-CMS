import { PackageManifestResponseModel } from '@umbraco-cms/backoffice/backend-api';

export type UmbPackage = PackageManifestResponseModel;

export type PackageManifestResponse = UmbPackage[];

export type UmbPackageWithMigrationStatus = UmbPackage & {
	hasPendingMigrations: boolean;
};
