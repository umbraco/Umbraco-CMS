export function baseUrl(): string {
	if (typeof document !== 'undefined') {
		const baseElems = document.getElementsByTagName('base');
		if (baseElems.length) {
			return baseElems[0].href.slice(0, -1);
		}
	}

	return '';
}
