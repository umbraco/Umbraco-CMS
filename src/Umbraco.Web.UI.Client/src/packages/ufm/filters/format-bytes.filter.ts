import { UmbUfmFilterBase } from './base.filter.js';
import { formatBytes } from '@umbraco-cms/backoffice/utils';

class UmbUfmFormatBytesFilterApi extends UmbUfmFilterBase {
	filter(str?: string) {
		if (str === undefined || str === null || !str.length) return '';
		return formatBytes(Number(str));
	}
}

export { UmbUfmFormatBytesFilterApi as api };
