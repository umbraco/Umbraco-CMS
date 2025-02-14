import type { UmbDataMapper } from './types.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestDataMapper<MetaType extends MetaDataMapper = MetaDataMapper>
	extends ManifestApi<UmbDataMapper> {
	type: 'dataMapper';
	identifier: string;
	meta: MetaType;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaDataMapper {}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestDataMapper: ManifestDataMapper;
	}
}
