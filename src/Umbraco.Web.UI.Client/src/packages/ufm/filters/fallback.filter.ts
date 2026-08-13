import { UmbUfmFilterBase } from './base.filter.js';

class UmbUfmFallbackFilterApi extends UmbUfmFilterBase {
	filter(str: string, fallback: string) {
		return str === null || str === undefined || str === '' ? fallback : str;
	}
}

export { UmbUfmFallbackFilterApi as api };
