import type { UmbUfmFilterApi } from '../extensions/ufm-filter.extension.js';

export abstract class UmbUfmFilterBase implements UmbUfmFilterApi {
	abstract filter(...args: Array<unknown>): string | undefined | null;
	destroy() {}
}
