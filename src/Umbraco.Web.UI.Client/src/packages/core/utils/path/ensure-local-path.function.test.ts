import { ensureLocalPath } from './ensure-local-path.function.js';
import { expect } from '@open-wc/testing';

describe('ensureLocalPath', () => {
	it('should return the same URL if it is a local URL', () => {
		const localUrl = new URL('/test', window.location.origin);
		expect(ensureLocalPath(localUrl).href).to.eq(localUrl.href);
	});

	it('should return the fallback URL if the input URL is not a local URL', () => {
		const nonLocalUrl = new URL('https://example.com/test');
		const fallbackUrl = new URL('http://localhost/fallback');
		expect(ensureLocalPath(nonLocalUrl, fallbackUrl).href).to.eq(fallbackUrl.href);
	});

	it('should return the same URL if it is a local path', () => {
		const localPath = '/test';
		expect(ensureLocalPath(localPath).pathname).to.eq(localPath);
	});

	it('should return the fallback URL if the input path is not a local path', () => {
		const nonLocalPath = 'https://example.com/test';
		const fallbackUrl = new URL('http://localhost/fallback');
		expect(ensureLocalPath(nonLocalPath, fallbackUrl).href).to.eq(fallbackUrl.href);
	});
});
