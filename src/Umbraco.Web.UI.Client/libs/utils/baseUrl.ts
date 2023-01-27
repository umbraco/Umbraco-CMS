export function baseUrl(): string {
	if (typeof document !== 'undefined') {
		const baseElems = document.getElementsByTagName('base');
		if (baseElems.length && baseElems[0].hasAttribute('href')) {
			return baseElems[0].href.slice(0, -1);
		}
	}

	return '';
}
