import { UmbUfmFilterBase } from './base.filter.js';

class UmbUfmTruncateFilterApi extends UmbUfmFilterBase {
	filter(str: string, length: number, tail?: string) {
		if (typeof str !== 'string' || !str.length) return str;
		if (tail === 'false') tail = '';
		if (tail === 'true') tail = '…';
		tail = !tail && tail !== '' ? '…' : tail;

		return str.slice(0, length).trim() + tail;
	}
}

export { UmbUfmTruncateFilterApi as api };
