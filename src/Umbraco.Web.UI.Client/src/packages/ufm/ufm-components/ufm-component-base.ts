import type { Tokens } from '@umbraco-cms/backoffice/external/marked';
import type { UmbUfmComponentApi } from '@umbraco-cms/backoffice/extension-registry';

export abstract class UmbUfmComponentBase implements UmbUfmComponentApi {
	abstract render(token: Tokens.Generic): string | undefined;
	destroy() {}
}
