import { appendCacheBust } from './append-cache-bust.function.js';
import { expect } from '@open-wc/testing';

describe('appendCacheBust', () => {
	it('appends the server cache-buster to a clean /App_Plugins path', () => {
		expect(appendCacheBust('/App_Plugins/MyPkg/index.js', '1.2.3-a1b2c3d')).to.equal(
			'/App_Plugins/MyPkg/index.js?umb__rnd=1.2.3-a1b2c3d',
		);
	});

	it('matches the /App_Plugins root case-insensitively', () => {
		expect(appendCacheBust('/app_plugins/MyPkg/index.js', '1.2.3')).to.equal('/app_plugins/MyPkg/index.js?umb__rnd=1.2.3');
	});

	it('inserts the query before a fragment', () => {
		expect(appendCacheBust('/App_Plugins/MyPkg/index.js#frag', '1.2.3')).to.equal(
			'/App_Plugins/MyPkg/index.js?umb__rnd=1.2.3#frag',
		);
	});

	it('escapes the cache-buster value', () => {
		expect(appendCacheBust('/App_Plugins/MyPkg/index.js', 'a/b c')).to.equal(
			'/App_Plugins/MyPkg/index.js?umb__rnd=a%2Fb%20c',
		);
	});

	['', undefined, null].forEach((cacheBuster) => {
		it(`leaves the url unchanged when the cache-buster is ${JSON.stringify(cacheBuster)}`, () => {
			const url = '/App_Plugins/MyPkg/index.js';
			expect(appendCacheBust(url, cacheBuster)).to.equal(url);
		});
	});

	[
		'/umbraco/backoffice/apps/app/index.js',
		'@umbraco-cms/backoffice/router',
		'https://cdn.example.com/pkg/index.js',
		'//cdn.example.com/pkg/index.js',
		'/App_PluginsFoo/index.js',
	].forEach((url) => {
		it(`leaves a non-/App_Plugins url unchanged: ${url}`, () => {
			expect(appendCacheBust(url, '1.2.3')).to.equal(url);
		});
	});

	it('skips a url that already has a query string (the token is resolved server-side)', () => {
		const url = '/App_Plugins/MyPkg/index.js?cb=%CACHE_BUSTER%';
		expect(appendCacheBust(url, '1.2.3')).to.equal(url);
	});
});
