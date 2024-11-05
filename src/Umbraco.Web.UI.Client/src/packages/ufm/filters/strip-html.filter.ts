import { UmbUfmFilterBase } from './base.filter.js';

class UmbUfmStripHtmlFilterApi extends UmbUfmFilterBase {
	filter(value: string | { markup: string } | undefined | null) {
		if (!value) return '';

		const markup = typeof value === 'object' && Object.hasOwn(value, 'markup') ? value.markup : (value as string);
		const parser = new DOMParser();
		const doc = parser.parseFromString(markup, 'text/html');

		return doc.body.textContent ?? '';
	}
}

export { UmbUfmStripHtmlFilterApi as api };
