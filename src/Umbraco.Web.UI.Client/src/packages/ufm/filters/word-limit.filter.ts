import { UmbUfmFilterBase } from './base.filter.js';

class UmbUfmWordLimitFilterApi extends UmbUfmFilterBase {
	filter(str: string, limit: number) {
		const words = str?.split(/\s+/) ?? [];
		return limit && words.length > limit ? words.slice(0, limit).join(' ') : str;
	}
}

export { UmbUfmWordLimitFilterApi as api };
