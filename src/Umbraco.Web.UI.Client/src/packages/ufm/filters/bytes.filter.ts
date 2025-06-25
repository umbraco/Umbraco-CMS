import { UmbUfmFilterBase } from './base.filter.js';
import { formatBytes } from '@umbraco-cms/backoffice/utils';

class UmbUfmBytesFilterApi extends UmbUfmFilterBase {
	filter(str?: string) {
		if (str === undefined || str === null || !str.length) return '';
		return formatBytes(Number(str));
	}
}

export { UmbUfmBytesFilterApi as api };
