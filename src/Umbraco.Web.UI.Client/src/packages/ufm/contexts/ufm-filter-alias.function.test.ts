import { expect } from '@open-wc/testing';
import { umbResolveUfmFilterAlias } from './ufm-filter-alias.function.js';

// Note: the UmbDeprecation.warn() side effect and the "warn once per alias" dedup are not tested
// here. The test harness proxies console.warn in a way that makes spying on it unreliable, and
// the dedup logic (a Set.has/add) is trivially correct by inspection.
describe('umbResolveUfmFilterAlias', () => {
	it('maps deprecated kebab-case aliases to their camelCase equivalents', () => {
		expect(umbResolveUfmFilterAlias('strip-html')).to.equal('stripHtml');
		expect(umbResolveUfmFilterAlias('title-case')).to.equal('titleCase');
		expect(umbResolveUfmFilterAlias('word-limit')).to.equal('wordLimit');
	});

	it('passes camelCase aliases through unchanged', () => {
		expect(umbResolveUfmFilterAlias('stripHtml')).to.equal('stripHtml');
	});

	it('passes unknown aliases through unchanged', () => {
		expect(umbResolveUfmFilterAlias('custom-thing')).to.equal('custom-thing');
	});
});
