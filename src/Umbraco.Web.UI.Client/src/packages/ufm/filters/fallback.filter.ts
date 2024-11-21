import { UmbUfmFilterBase } from './base.filter.js';

class UmbUfmFallbackFilterApi extends UmbUfmFilterBase {
	filter(str: string, fallback: string) {
		return typeof str !== 'string' || str ? str : fallback;
	}
}

export { UmbUfmFallbackFilterApi as api };
