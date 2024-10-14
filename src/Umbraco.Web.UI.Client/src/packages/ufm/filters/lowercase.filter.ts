import { UmbUfmFilterBase } from '../types.js';

class UmbUfmLowercaseFilterApi extends UmbUfmFilterBase {
	filter(str?: string) {
		return str?.toLocaleLowerCase();
	}
}

export { UmbUfmLowercaseFilterApi as api };
