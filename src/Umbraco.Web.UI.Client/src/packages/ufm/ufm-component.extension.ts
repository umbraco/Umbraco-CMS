import type { ManifestApi, UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UfmToken } from './plugins/index.js';

export interface UmbUfmComponentApi extends UmbApi {
	render(token: UfmToken): string | undefined;
}

export interface MetaUfmComponent {
	alias: string;
	marker?: string;
}

export interface ManifestUfmComponent extends ManifestApi<UmbUfmComponentApi> {
	type: 'ufmComponent';
	meta: MetaUfmComponent;
}

declare global {
	interface UmbExtensionManifestMap {
		umbUfmComponent: ManifestUfmComponent;
	}
}
