import type { ManifestApi, UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UfmToken } from '@umbraco-cms/backoffice/ufm';

export interface UmbUfmComponentApi extends UmbApi {
	render(token: UfmToken): string | undefined;
}

export interface MetaUfmComponent {
	marker: string;
}

export interface ManifestUfmComponent extends ManifestApi<UmbUfmComponentApi> {
	type: 'ufmComponent';
	meta: MetaUfmComponent;
}
