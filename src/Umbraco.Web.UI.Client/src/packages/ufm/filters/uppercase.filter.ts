import { UmbUfmFilterBase } from './base.filter.js';

class UmbUfmUppercaseFilterApi extends UmbUfmFilterBase {
	filter(str?: string) {
		return str?.toLocaleUpperCase();
	}
}

export { UmbUfmUppercaseFilterApi as api };
