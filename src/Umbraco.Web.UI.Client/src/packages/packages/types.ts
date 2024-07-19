import type { ManifestResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export type UmbPackage = ManifestResponseModel;

export type PackageManifestResponse = UmbPackage[];

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
