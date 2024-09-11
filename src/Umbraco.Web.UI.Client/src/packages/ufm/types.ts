import type { UmbUfmFilterApi } from '@umbraco-cms/backoffice/extension-registry';

export abstract class UmbUfmFilterBase implements UmbUfmFilterApi {
	abstract filter(...args: Array<unknown>): string | undefined | null;
	destroy() {}
}
