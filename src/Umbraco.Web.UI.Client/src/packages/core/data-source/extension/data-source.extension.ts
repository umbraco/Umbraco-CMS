import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestDataSource extends ManifestApi<any> {
	type: 'dataSource';
	dataSourceType: string;
	meta: MetaDataSource;
}

export interface MetaDataSource {
	label: string;
	description?: string;
	icon?: string;
}

declare global {
	interface UmbExtensionManifestMap {
		UmbDataSource: ManifestDataSource;
	}
}
