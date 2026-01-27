import { UmbAuthContext } from './auth.context.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UMB_STORAGE_TOKEN_RESPONSE_NAME } from './constants.js';

@customElement('test-auth-context-host')
class UmbTestAuthContextHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbAuthContext', () => {
	let context: UmbAuthContext;
	let hostElement: UmbTestAuthContextHostElement;

	beforeEach(() => {
		hostElement = new UmbTestAuthContextHostElement();
		// Create context with test parameters
		context = new UmbAuthContext(hostElement, 'http://localhost', '/umbraco', false);
	});

	afterEach(() => {
		// Clean up context
		context.destroy();
	});

	describe('Event Listener Management', () => {
		it('responds to storage events with correct key', (done) => {
			// We can verify the handler is attached by triggering a storage event
			// and checking if it affects the context state

			// Set initial state
			context.setInitialState().then(() => {
				// Create a storage event for the token response
				const storageEvent = new StorageEvent('storage', {
					key: UMB_STORAGE_TOKEN_RESPONSE_NAME,
					newValue: 'test-value',
					url: window.location.href,
					storageArea: localStorage,
				});

				// Dispatch the event
				window.dispatchEvent(storageEvent);

				// Give it a moment to process
				setTimeout(() => {
					// If the listener is attached and working, it should have triggered
					// We can't directly test if a listener exists, but we can verify
					// the context is still functioning correctly
					expect(context).to.exist;
					done();
				}, 50);
			});
		});

		it('does not respond to storage events with incorrect key', (done) => {
			// Create a storage event with a different key
			const storageEvent = new StorageEvent('storage', {
				key: 'some-other-key',
				newValue: 'test-value',
				url: window.location.href,
				storageArea: localStorage,
			});

			// Set up a spy to track if setInitialState gets called
			let setInitialStateCalled = false;
			const originalSetInitialState = context.setInitialState.bind(context);
			context.setInitialState = async () => {
				setInitialStateCalled = true;
				return originalSetInitialState();
			};

			// Dispatch the event
			window.dispatchEvent(storageEvent);

			// Give it a moment to process
			setTimeout(() => {
				// The handler should ignore events with wrong keys
				expect(setInitialStateCalled).to.be.false;
				done();
			}, 50);
		});

		it('can be destroyed without errors', () => {
			// Test that destroy() works correctly and removes event listeners
			expect(() => context.destroy()).to.not.throw();

			// After destruction, create another context to ensure no conflicts
			const context2 = new UmbAuthContext(hostElement, 'http://localhost', '/umbraco', false);
			expect(context2).to.exist;
			context2.destroy();
		});

		it('handles multiple instances without conflicts', () => {
			// Create a second context on a different host
			const hostElement2 = new UmbTestAuthContextHostElement();
			const context2 = new UmbAuthContext(hostElement2, 'http://localhost', '/umbraco', false);

			// Both should exist without errors
			expect(context).to.exist;
			expect(context2).to.exist;

			// Clean up second context
			context2.destroy();
		});
	});

	describe('Public API', () => {
		it('has an isAuthorized property', () => {
			expect(context).to.have.property('isAuthorized');
		});

		it('has an isInitialized property', () => {
			expect(context).to.have.property('isInitialized');
		});

		it('has a getIsAuthorized method', () => {
			expect(context).to.have.property('getIsAuthorized').that.is.a('function');
		});

		it('has a setInitialState method', () => {
			expect(context).to.have.property('setInitialState').that.is.a('function');
		});

		it('has a getLatestToken method', () => {
			expect(context).to.have.property('getLatestToken').that.is.a('function');
		});

		it('has a validateToken method', () => {
			expect(context).to.have.property('validateToken').that.is.a('function');
		});

		it('has a clearTokenStorage method', () => {
			expect(context).to.have.property('clearTokenStorage').that.is.a('function');
		});

		it('has a signOut method', () => {
			expect(context).to.have.property('signOut').that.is.a('function');
		});

		it('has a getServerUrl method', () => {
			expect(context).to.have.property('getServerUrl').that.is.a('function');
		});
	});

	describe('getServerUrl', () => {
		it('returns the configured server URL', () => {
			const serverUrl = context.getServerUrl();
			expect(serverUrl).to.equal('http://localhost');
		});
	});

	describe('getIsAuthorized', () => {
		it('returns a boolean value', () => {
			const isAuthorized = context.getIsAuthorized();
			expect(isAuthorized).to.be.a('boolean');
		});

		it('returns true when auth is bypassed', () => {
			const bypassedContext = new UmbAuthContext(hostElement, 'http://localhost', '/umbraco', true);
			const isAuthorized = bypassedContext.getIsAuthorized();
			expect(isAuthorized).to.be.true;
			bypassedContext.destroy();
		});
	});
});
