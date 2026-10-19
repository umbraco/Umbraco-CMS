import { isReturnableRoute } from './returnable-route.function.js';
import { expect } from '@open-wc/testing';

describe('isReturnableRoute', () => {
	// Only routes behind the auth guard are worth returning to. `upgrade` matters as much as the
	// section routes: when the backend requires an upgrade the client routes there, so login has to
	// come back. The rest render without a session, so returning would show the login screen again.
	const cases: Array<[pathname: string, backofficePath: string, expected: boolean]> = [
		// Back office served at /umbraco
		['/umbraco/section/content/workspace/document/edit/123', '/umbraco', true],
		['/umbraco/upgrade', '/umbraco', true],
		['/umbraco/preview', '/umbraco', true],
		['/umbraco/logout', '/umbraco', false],
		['/umbraco/error', '/umbraco', false],
		['/umbraco/auth-callback', '/umbraco', false],
		['/umbraco/install', '/umbraco', false],
		['/umbraco', '/umbraco', false],
		['/umbraco/', '/umbraco', false],
		// Back office served at the site root, e.g. a separate front-end host
		['/section/content', '/', true],
		['/logout', '/', false],
		['/', '/', false],
	];

	cases.forEach(([pathname, backofficePath, expected]) => {
		it(`${expected ? 'returns to' : 'skips'} ${pathname} (back office at ${backofficePath})`, () => {
			expect(isReturnableRoute(pathname, backofficePath)).to.equal(expected);
		});
	});

	// A section route may legitimately end in a word that also names a boot route; only the
	// top-level segment decides, so the deep link survives.
	it('keeps a deep link whose trailing segment happens to match a boot route', () => {
		expect(isReturnableRoute('/umbraco/section/settings/logviewer/error', '/umbraco')).to.be.true;
	});
});
