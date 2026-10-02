import { UmbUfmFilterBase } from './base.filter.js';

class UmbUfmFallbackFilterApi extends UmbUfmFilterBase {
	filter(str: string | null | undefined, fallback?: string) {
		return str === null || str === undefined || str === '' ? fallback : str;
	}
}

export { UmbUfmFallbackFilterApi as api };
