import type { UmbDataMapping } from './types.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestDataMapping<MetaType extends MetaDataMapping = MetaDataMapping>
	extends ManifestApi<UmbDataMapping> {
	type: 'dataMapping';
	identifier: string;
	meta: MetaType;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaDataMapping {}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestDataMapping: ManifestDataMapping;
	}
}
