import type { UmbUfmFilterApi } from '../ufm-filter.extension.js';

export abstract class UmbUfmFilterBase implements UmbUfmFilterApi {
	abstract filter(...args: Array<unknown>): string | undefined | null;
	destroy() {}
}
