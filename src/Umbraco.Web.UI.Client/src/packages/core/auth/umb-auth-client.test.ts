import { UmbAuthClient } from './umb-auth-client.js';
import type { UmbAuthClientEndpoints } from './umb-auth-client.js';
import { expect } from '@open-wc/testing';

const TEST_ENDPOINTS: UmbAuthClientEndpoints = {
	authorizationEndpoint: 'http://localhost/authorize',
	tokenEndpoint: 'http://localhost/token',
	revocationEndpoint: 'http://localhost/revoke',
	linkEndpoint: 'http://localhost/link-login',
	linkKeyEndpoint: 'http://localhost/link-login-key',
	unlinkEndpoint: 'http://localhost/unlink-login',
};

describe('UmbAuthClient', () => {
	let client: UmbAuthClient;

	beforeEach(() => {
		client = new UmbAuthClient(TEST_ENDPOINTS, 'http://localhost/oauth_complete');
	});

	describe('PKCE generation', () => {
		it('generates a code_verifier and state when building authorization URL', async () => {
			const url = await client.buildAuthorizationUrl('Umbraco');

			expect(client.codeVerifier).to.be.a('string');
			expect(client.codeVerifier!.length).to.be.greaterThan(40);
			expect(client.state).to.be.a('string');
			expect(client.state!.length).to.be.greaterThan(0);
			expect(url).to.be.a('string');
		});

		it('includes PKCE parameters in the authorization URL', async () => {
			const url = await client.buildAuthorizationUrl('Umbraco');

			expect(url).to.contain('code_challenge=');
			expect(url).to.contain('code_challenge_method=S256');
			expect(url).to.contain('response_type=code');
			expect(url).to.contain('client_id=umbraco-back-office');
		});

		it('includes identity_provider when not Umbraco', async () => {
			const url = await client.buildAuthorizationUrl('Google');
			expect(url).to.contain('identity_provider=Google');
		});

		it('does not include identity_provider for Umbraco', async () => {
			const url = await client.buildAuthorizationUrl('Umbraco');
			expect(url).to.not.contain('identity_provider=');
		});

		it('includes login_hint when provided', async () => {
			const url = await client.buildAuthorizationUrl('Umbraco', 'test@example.com');
			expect(url).to.contain('login_hint=test%40example.com');
		});

		it('generates different code_verifiers on each call', async () => {
			await client.buildAuthorizationUrl('Umbraco');
			const firstVerifier = client.codeVerifier;

			await client.buildAuthorizationUrl('Umbraco');
			const secondVerifier = client.codeVerifier;

			expect(firstVerifier).to.not.equal(secondVerifier);
		});
	});

	describe('PKCE generation in insecure contexts', () => {
		it('derives the same S256 code challenge when crypto.subtle is unavailable', async () => {
			// http://<ip> origins are insecure contexts where the browser removes crypto.subtle;
			// simulate that by shadowing the prototype getter on the crypto instance.
			const subtle = crypto.subtle;
			let url: string;
			Object.defineProperty(crypto, 'subtle', { value: undefined, configurable: true });
			try {
				expect(crypto.subtle, 'crypto.subtle should be shadowed for this test').to.be.undefined;
				url = await client.buildAuthorizationUrl('Umbraco');
			} finally {
				// Deleting the own property restores the prototype getter.
				delete (crypto as { subtle?: SubtleCrypto }).subtle;
			}

			const challenge = new URL(url).searchParams.get('code_challenge');
			const expectedHash = await subtle.digest('SHA-256', new TextEncoder().encode(client.codeVerifier!));
			const expectedChallenge = btoa(String.fromCharCode(...new Uint8Array(expectedHash)))
				.replace(/\+/g, '-')
				.replace(/\//g, '_')
				.replace(/=/g, '');

			expect(challenge).to.equal(expectedChallenge);
		});
	});

	describe('clearPkceState', () => {
		it('clears the stored PKCE state', async () => {
			await client.buildAuthorizationUrl('Umbraco');
			expect(client.codeVerifier).to.be.a('string');
			expect(client.state).to.be.a('string');

			client.clearPkceState();
			expect(client.codeVerifier).to.be.undefined;
			expect(client.state).to.be.undefined;
		});
	});
});
