import { UmbUfmFilterBase } from './base.filter.js';

class UmbUfmLowercaseFilterApi extends UmbUfmFilterBase {
	filter(str?: string) {
		return str?.toLocaleLowerCase();
	}
}

export { UmbUfmLowercaseFilterApi as api };
