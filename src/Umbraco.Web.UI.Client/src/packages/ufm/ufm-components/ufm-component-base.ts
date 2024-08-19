import type { UfmToken } from '../plugins/marked-ufm.plugin.js';
import type { UmbUfmComponentApi } from '@umbraco-cms/backoffice/extension-registry';

export abstract class UmbUfmComponentBase implements UmbUfmComponentApi {
	abstract render(token: UfmToken): string | undefined;
	destroy() {}
}
