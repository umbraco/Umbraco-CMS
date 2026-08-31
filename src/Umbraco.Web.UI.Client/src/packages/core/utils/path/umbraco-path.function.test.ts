import { umbracoPath } from './umbraco-path.function.js';
import { expect } from '@open-wc/testing';

describe('umbracoPath', () => {
	it('prefixes the path with /umbraco/management/api/v1 by default', () => {
		expect(umbracoPath('/document')).to.equal('/umbraco/management/api/v1/document');
	});

	it('uses the supplied version when one is given', () => {
		expect(umbracoPath('/document', 'v2')).to.equal('/umbraco/management/api/v2/document');
	});

	it('returns the bare prefix for an empty path', () => {
		expect(umbracoPath('')).to.equal('/umbraco/management/api/v1');
	});

	it('appends the path verbatim — no slash normalisation', () => {
		expect(umbracoPath('document')).to.equal('/umbraco/management/api/v1document');
	});
});
