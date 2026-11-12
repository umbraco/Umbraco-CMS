import type { ManifestFilter, MetaFilter } from '../filter.extension.js';
import type { UmbDatalistDataSource } from '@umbraco-cms/backoffice/datalist-data-source';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface ManifestFilterSelectKind extends ManifestFilter {
	type: 'filter';
	kind: 'select';
	meta: MetaFilterSelect;
}

export interface MetaFilterSelect extends MetaFilter {
	datalistDataSource?: new (host: UmbControllerHost) => UmbDatalistDataSource;
	multiple?: boolean;
}

declare global {
	interface UmbExtensionManifestMap {
		umbFilterSelectKind: ManifestFilterSelectKind;
	}
}
