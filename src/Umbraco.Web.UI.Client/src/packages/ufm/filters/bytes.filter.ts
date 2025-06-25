import { UmbUfmFilterBase } from './base.filter.js';
import { formatBytes } from '@umbraco-cms/backoffice/utils';

class UmbUfmBytesFilterApi extends UmbUfmFilterBase {
	filter(str?: string, decimals?: number, kilo?: number, culture?: string): string {
		if (!str?.length) return '';
		return formatBytes(Number(str), { decimals, kilo, culture });
	}
}

export { UmbUfmBytesFilterApi as api };
