import { expect } from '@open-wc/testing';
import { directAuthProvider } from './direct-auth-provider.function.js';
import type { ManifestAuthProvider } from '@umbraco-cms/backoffice/auth';

function provider(forProviderName: string, autoRedirect?: boolean): ManifestAuthProvider {
	return {
		type: 'authProvider',
		alias: `Umb.Test.${forProviderName}`,
		name: forProviderName,
		forProviderName,
		meta: autoRedirect === undefined ? {} : { behavior: { autoRedirect } },
	} as unknown as ManifestAuthProvider;
}

describe('directAuthProvider', () => {
	it('returns the only provider', () => {
		expect(directAuthProvider([provider('Umbraco')])?.forProviderName).to.equal('Umbraco');
	});

	// Regression guard: the cookie-auth rewrite of the auth guard dropped the autoRedirect check, so a
	// site configuring it met the provider chooser instead of being sent to its provider.
	it('returns a provider that asks to be gone to directly, even alongside others', () => {
		const providers = [provider('Umbraco'), provider('Umbraco.Id', true), provider('Other')];

		expect(directAuthProvider(providers)?.forProviderName).to.equal('Umbraco.Id');
	});

	it('returns nothing when several are registered and none asks', () => {
		expect(directAuthProvider([provider('Umbraco'), provider('Other')])).to.be.undefined;
	});

	it('treats autoRedirect: false as no preference', () => {
		expect(directAuthProvider([provider('Umbraco', false), provider('Other', false)])).to.be.undefined;
	});

	it('honours the single provider even when it declines the redirect', () => {
		// There is no one else to choose, so the chooser would be a pointless click.
		expect(directAuthProvider([provider('Umbraco', false)])?.forProviderName).to.equal('Umbraco');
	});

	it('returns nothing when no providers are registered', () => {
		expect(directAuthProvider([])).to.be.undefined;
	});
});
