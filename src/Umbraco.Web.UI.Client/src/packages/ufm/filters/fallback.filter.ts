import { UmbUfmFilterBase } from '../types.js';

class UmbUfmFallbackFilterApi extends UmbUfmFilterBase {
	filter(str: string, fallback: string) {
		return typeof str !== 'string' || str ? str : fallback;
	}
}

export { UmbUfmFallbackFilterApi as api };
