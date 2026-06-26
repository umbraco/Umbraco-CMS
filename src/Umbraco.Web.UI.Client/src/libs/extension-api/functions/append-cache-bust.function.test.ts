import { appendCacheBust } from './append-cache-bust.function.js';
import { expect } from '@open-wc/testing';

describe('appendCacheBust', () => {
	it('appends the version and host cache-buster to a clean /App_Plugins path', () => {
		expect(appendCacheBust('/App_Plugins/MyPkg/index.js', '1.2.3', 'seed', true)).to.equal(
			'/App_Plugins/MyPkg/index.js?v=1.2.3&umb__rnd=seed',
		);
	});

	it('appends only the version when there is no host cache-buster', () => {
		expect(appendCacheBust('/App_Plugins/MyPkg/index.js', '1.2.3', '', true)).to.equal(
			'/App_Plugins/MyPkg/index.js?v=1.2.3',
		);
	});

	it('appends only the host cache-buster when the package has no version', () => {
		expect(appendCacheBust('/App_Plugins/MyPkg/index.js', undefined, 'seed', true)).to.equal(
			'/App_Plugins/MyPkg/index.js?umb__rnd=seed',
		);
	});

	it('leaves a clean path unchanged when there is neither a version nor a cache-buster', () => {
		expect(appendCacheBust('/App_Plugins/MyPkg/index.js', undefined, undefined, true)).to.equal(
			'/App_Plugins/MyPkg/index.js',
		);
	});

	it('escapes the values', () => {
		expect(appendCacheBust('/App_Plugins/MyPkg/index.js', '1.0 beta', 'a/b', true)).to.equal(
			'/App_Plugins/MyPkg/index.js?v=1.0%20beta&umb__rnd=a%2Fb',
		);
	});

	it('matches the /App_Plugins root case-insensitively', () => {
		expect(appendCacheBust('/app_plugins/MyPkg/index.js', '1.2.3', undefined, true)).to.equal(
			'/app_plugins/MyPkg/index.js?v=1.2.3',
		);
	});

	it('inserts the query before a fragment', () => {
		expect(appendCacheBust('/App_Plugins/MyPkg/index.js#frag', '1.2.3', undefined, true)).to.equal(
			'/App_Plugins/MyPkg/index.js?v=1.2.3#frag',
		);
	});

	[
		'/umbraco/backoffice/apps/app/index.js',
		'@umbraco-cms/backoffice/router',
		'https://cdn.example.com/pkg/index.js',
		'//cdn.example.com/pkg/index.js',
		'/App_PluginsFoo/index.js',
	].forEach((url) => {
		it(`leaves a non-/App_Plugins url unchanged: ${url}`, () => {
			expect(appendCacheBust(url, '1.2.3', 'seed', true)).to.equal(url);
		});
	});

	it('skips a url that already has a query string', () => {
		const url = '/App_Plugins/MyPkg/index.js?foo=1';
		expect(appendCacheBust(url, '1.2.3', 'seed', true)).to.equal(url);
	});

	it('does not auto-stamp a clean path when autoStamp is disabled', () => {
		const url = '/App_Plugins/MyPkg/index.js';
		expect(appendCacheBust(url, '1.2.3', 'seed', false)).to.equal(url);
	});

	it('resolves an explicit %CACHE_BUSTER% token to the version', () => {
		expect(appendCacheBust('/App_Plugins/MyPkg/index.js?cb=%CACHE_BUSTER%', '1.2.3', 'seed', true)).to.equal(
			'/App_Plugins/MyPkg/index.js?cb=1.2.3',
		);
	});

	it('resolves an explicit %CACHE_BUSTER% token to the cache-buster when there is no version', () => {
		expect(appendCacheBust('/App_Plugins/MyPkg/index.js?cb=%CACHE_BUSTER%', undefined, 'seed', true)).to.equal(
			'/App_Plugins/MyPkg/index.js?cb=seed',
		);
	});

	it('resolves an explicit %CACHE_BUSTER% token on any host, regardless of autoStamp', () => {
		expect(appendCacheBust('https://cdn.example.com/pkg/index.js?cb=%CACHE_BUSTER%', '1.2.3', 'seed', false)).to.equal(
			'https://cdn.example.com/pkg/index.js?cb=1.2.3',
		);
	});
});
