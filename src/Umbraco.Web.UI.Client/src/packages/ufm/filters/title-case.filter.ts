import { UmbUfmFilterBase } from './base.filter.js';

class UmbUfmTitleCaseFilterApi extends UmbUfmFilterBase {
	filter(str?: string) {
		return str?.replace(/\w\S*/g, (txt) => txt.charAt(0).toUpperCase() + txt.substring(1).toLowerCase());
	}
}

export { UmbUfmTitleCaseFilterApi as api };
