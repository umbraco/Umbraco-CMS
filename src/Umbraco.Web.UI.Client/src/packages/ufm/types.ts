import type { UmbUfmFilterApi } from './ufm-filter.extension.js';

// TODO: This is not a type? So it should ideally be move to a different file. [NL]
export abstract class UmbUfmFilterBase implements UmbUfmFilterApi {
	abstract filter(...args: Array<unknown>): string | undefined | null;
	destroy() {}
}
