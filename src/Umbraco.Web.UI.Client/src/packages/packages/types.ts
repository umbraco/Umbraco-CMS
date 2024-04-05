import type { ManifestResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export type UmbPackage = ManifestResponseModel;

export type PackageManifestResponse = UmbPackage[];

export type UmbPackageWithMigrationStatus = UmbPackage & {
	hasPendingMigrations: boolean;
};
