import type { UmbUfmFilterApi } from './ufm-filter.extension.js';

// TODO: this is not a type, in TypeScript world, as it is an actual class. So it should be moved elsewhere [NL]
export abstract class UmbUfmFilterBase implements UmbUfmFilterApi {
	abstract filter(...args: Array<unknown>): string | undefined | null;
	destroy() {}
}
