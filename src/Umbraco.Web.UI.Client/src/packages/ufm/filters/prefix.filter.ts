import { UmbUfmFilterBase } from './base.filter.js';

class UmbUfmPrefixFilterApi extends UmbUfmFilterBase {

  	filter(str?: string, prefix: string = '') {
        if (!str) return '';
		return `${prefix} ${str}`;
	}
}
export { UmbUfmPrefixFilterApi as api };