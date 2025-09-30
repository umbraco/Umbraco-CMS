import type { ManifestResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export type UmbPackage = ManifestResponseModel;

/**
 * Represents the response from the package manifest endpoint.
 * This type is used to define the structure of the package manifest response.
 */
export type UmbPackageManifestResponse = UmbPackage[];

/**
 * Represents the response from the package manifest endpoint.
 * @deprecated Use `UmbPackageManifestResponse` instead. This will be removed in Umbraco 18.
 */
// eslint-disable-next-line @typescript-eslint/naming-convention
export type PackageManifestResponse = UmbPackageManifestResponse;

export type UmbPackageWithMigrationStatus = UmbPackage & {
	hasPendingMigrations: boolean;
};

export type UmbCreatedPackage = { unique: string; name: string };

export type UmbCreatedPackages = { items: Array<UmbCreatedPackage>; total: number };

export type UmbCreatedPackageDefinition = UmbCreatedPackage & {
	packagePath: string;
	contentNodeId?: string | null;
	contentLoadChildNodes: boolean;
	mediaIds: Array<string>;
	mediaLoadChildNodes: boolean;
	documentTypes: Array<string>;
	mediaTypes: Array<string>;
	dataTypes: Array<string>;
	templates: Array<string>;
	partialViews: Array<string>;
	stylesheets: Array<string>;
	scripts: Array<string>;
	languages: Array<string>;
	dictionaryItems: Array<string>;
};
