import { UmbUfmFilterBase } from '../types.js';

class UmbUfmUppercaseFilterApi extends UmbUfmFilterBase {
	filter(str?: string) {
		return str?.toLocaleUpperCase();
	}
}

export { UmbUfmUppercaseFilterApi as api };
