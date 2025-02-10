import type { UmbDataMapper } from './types.js';
import type { ManifestApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface Manifest$TypeDataMapper<MetaType extends MetaDataMapper = MetaDataMapper>
	extends ManifestApi<UmbDataMapper>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: '$typeDataMapper';
	meta: MetaType;
	from$type: string;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaDataMapper {}

declare global {
	interface UmbExtensionManifestMap {
		umbManifest$TypeDataMapper: Manifest$TypeDataMapper;
	}
}
