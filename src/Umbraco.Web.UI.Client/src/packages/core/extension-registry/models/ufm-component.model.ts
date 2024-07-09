import type { ManifestApi, UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { Tokens } from '@umbraco-cms/backoffice/external/marked';

export interface UmbUfmComponentApi extends UmbApi {
	render(token: Tokens.Generic): string | undefined;
}

export interface MetaUfmComponent {
	marker: string;
}

export interface ManifestUfmComponent extends ManifestApi<UmbUfmComponentApi> {
	type: 'ufmComponent';
	meta: MetaUfmComponent;
}
