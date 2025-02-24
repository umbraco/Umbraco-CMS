import type { UmbDataSourceDataMapping } from './types.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestDataSourceDataMapping<MetaType extends MetaDataSourceDataMapping = MetaDataSourceDataMapping>
	extends ManifestApi<UmbDataSourceDataMapping> {
	type: 'dataSourceDataMapping';
	forDataSource: string;
	forDataModel: string;
	meta: MetaType;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaDataSourceDataMapping {}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestDataSourceDataMapping: ManifestDataSourceDataMapping;
	}
}
