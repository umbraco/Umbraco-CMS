import { onInit } from '../../packages/core/entry-point.js';
import { UmbAppErrorElement } from './app-error.element.js';
import { UmbAppAuthController } from './app-auth.controller.js';
import { UmbAppAuthElement } from './app-auth.element.js';
import { UmbAppOauthElement } from './app-oauth.element.js';
import { UmbNetworkConnectionStatusManager } from './network-connection-status.manager.js';
import type { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { UmbAuthContext } from '@umbraco-cms/backoffice/auth';
import { UmbServerConnection, UmbServerContext } from '@umbraco-cms/backoffice/server';
import { css, html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UUIIconRegistryEssential } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { Guard, UmbRoute } from '@umbraco-cms/backoffice/router';
import { pathWithoutBasePath } from '@umbraco-cms/backoffice/router';
import { RuntimeLevelModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbContextDebugController } from '@umbraco-cms/backoffice/debug';
import { UmbBundleExtensionInitializer, UmbServerExtensionRegistrator } from '@umbraco-cms/backoffice/extension-api';
import {
	UmbAppEntryPointExtensionInitializer,
	umbExtensionsRegistry,
} from '@umbraco-cms/backoffice/extension-registry';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { redirectToStoredPath } from '@umbraco-cms/backoffice/utils';
import { umbHttpClient } from '@umbraco-cms/backoffice/http-client';
import { UmbViewContext } from '@umbraco-cms/backoffice/view';

import './app-logo.element.js';

@customElement('umb-app')
export class UmbAppElement extends UmbLitElement {
	/**
	 * The base URL of the configured Umbraco server.
	 * @attr
	 * @remarks This is the base URL of the Umbraco server, not the base URL of the backoffice.
	 */
	@property({ type: String, attribute: 'server-url' })
	serverUrl = window.location.origin;

	/**
	 * The base path of the backoffice.
	 * @attr
	 */
	@property({ type: String, attribute: 'backoffice-path' })
	backofficePath = '/umbraco';

	/**
	 * Bypass authentication.
	 */
	@property({ type: Boolean, attribute: 'bypass-auth' })
	bypassAuth = false;

	/**
	 * Keep the user logged in by automatically refreshing the session before it expires.
	 * @attr
	 */
	@property({ type: Boolean, attribute: 'keep-user-logged-in' })
	keepUserLoggedIn = false;

	private _routes: UmbRoute[] = [
		{
			path: 'error',
			component: UmbAppErrorElement,
		},
		{
			path: 'install',
			component: () => import('../installer/installer.element.js'),
		},
		{
			path: 'oauth_complete',
			component: UmbAppOauthElement,
			setup: async (component) => {
				if (!this.#authContext) {
					(component as UmbAppOauthElement).failure = true;
					console.error('[Fatal] Auth context is not available');
					return;
				}

				const searchParams = new URLSearchParams(window.location.search);
				const hasCode = searchParams.has('code');
				if (!hasCode) {
					(component as UmbAppOauthElement).failure = true;
					console.error('[Fatal] No code in query parameters');
					return;
				}

				// Check that we are not already authorized
				if (this.#authContext.getIsAuthorized()) {
					redirectToStoredPath(this.backofficePath, true);
					return;
				}

				// Complete the authorization request (exchanges code, saves session, broadcasts to other tabs)
				try {
					const result = await this.#authContext.completeAuthorizationRequest();

					if (result === null) {
						// No authorization was pending — redirect the user
						redirectToStoredPath(this.backofficePath, true);
						return;
					}

					// For redirect flows (no popup), navigate to the stored path.
					// Use force=true for a full page navigation so the new page
					// runs setInitialState() with the fresh httpOnly cookies.
					redirectToStoredPath(this.backofficePath, true);
				} catch {
					(component as UmbAppOauthElement).failure = true;
					console.error('[Fatal] Authorization request failed');
				}
			},
		},
		{
			path: 'upgrade',
			component: () => import('../upgrader/upgrader.element.js'),
			guards: [this.#isAuthorizedGuard()],
		},
		{
			path: 'preview',
			component: () => import('../preview/preview.element.js'),
			guards: [this.#isAuthorizedGuard()],
		},
		{
			path: 'logout',
			component: UmbAppAuthElement,
			setup: () => {
				this.#authContext?.clearTokenStorage();
			},
		},
		{
			path: '**',
			component: () => import('../backoffice/backoffice.element.js'),
			guards: [this.#isAuthorizedGuard()],
		},
	];

	#authContext?: typeof UMB_AUTH_CONTEXT.TYPE;
	#serverConnection?: UmbServerConnection;
	#authController = new UmbAppAuthController(this);

	constructor() {
		super();

		new UmbBundleExtensionInitializer(this, umbExtensionsRegistry);

		new UUIIconRegistryEssential().attach(this);

		new UmbContextDebugController(this);

		new UmbNetworkConnectionStatusManager(this);

		new UmbViewContext(this, null);
	}

	override connectedCallback(): void {
		super.connectedCallback();
		this.#setup();
	}

	async #setup() {
		this.#authContext = new UmbAuthContext(
			this,
			this.serverUrl,
			this.backofficePath,
			this.bypassAuth,
			this.keepUserLoggedIn,
		);
		this.#authContext.configureClient(umbHttpClient);

		this.#serverConnection = await new UmbServerConnection(this, this.serverUrl).connect();
		new UmbServerContext(this, {
			backofficePath: this.backofficePath,
			serverUrl: this.serverUrl,
			serverConnection: this.#serverConnection,
		});

		// Register Core extensions (this is specifically done here because we need these extensions to be registered before the application is initialized)
		onInit(this, umbExtensionsRegistry);

		// Register public extensions (login extensions)
		await new UmbServerExtensionRegistrator(this, umbExtensionsRegistry).registerPublicExtensions();
		const initializer = new UmbAppEntryPointExtensionInitializer(this, umbExtensionsRegistry);
		await firstValueFrom(initializer.loaded);

		// Try to initialise the auth flow and get the runtime status
		try {
			// If the runtime level is "install" or ?status=false is set, we should clear any cached tokens
			// else we should try and set the auth status
			const searchParams = new URLSearchParams(window.location.search);
			if (
				(searchParams.has('status') && searchParams.get('status') === 'false') ||
				this.#serverConnection.getStatus() === RuntimeLevelModel.INSTALL
			) {
				await this.#authContext.clearTokenStorage();
			} else {
				await this.#setAuthStatus();
			}

			// Initialise the router
			this.#redirect();
		} catch (error) {
			// If the auth flow fails, there is most likely something wrong with the connection to the backend server
			// and we should redirect to the error page
			let errorMsg =
				'An error occurred while trying to initialize the connection to the Umbraco server (check console for details)';

			// Get the type of the error and check http status codes
			if (error instanceof Error) {
				// If the error is a "TypeError" it means that the server is not reachable
				if (error.name === 'TypeError') {
					errorMsg = 'The Umbraco server is unreachable (check console for details)';
				}
			}

			// Log the error
			console.error(errorMsg, error);

			// Redirect to the error page
			this.#errorPage(errorMsg, error);
		}
	}

	async #setAuthStatus() {
		if (this.bypassAuth) return;

		if (!this.#authContext) {
			throw new Error('[Fatal] AuthContext requested before it was initialized');
		}

		// The oauth_complete popup must not call setInitialState(): a successful silent
		// refresh would set isAuthorized=true and cause the oauth_complete handler to
		// redirect the popup to the backoffice instead of completing the code exchange.
		// Other windows opened via window.open() (e.g. the preview window) DO need
		// setInitialState() so they can restore the session from a peer tab.
		const pathname = pathWithoutBasePath({ start: true, end: false });
		if (window.opener && pathname === '/oauth_complete') return;

		// Auth context configures umbHttpClient in its constructor, so we only need to set initial state
		await this.#authContext.setInitialState();
	}

	#redirect() {
		const pathname = pathWithoutBasePath({ start: true, end: false });

		// If we are on the oauth_complete or error page, we should not redirect
		if (pathname === '/oauth_complete' || pathname === '/error') {
			// Initialize the router
			history.replaceState(null, '', location.href);
			return;
		}

		switch (this.#serverConnection?.getStatus()) {
			case RuntimeLevelModel.INSTALL:
				history.replaceState(null, '', 'install');
				break;

			case RuntimeLevelModel.UPGRADE:
				history.replaceState(null, '', 'upgrade');
				break;

			case RuntimeLevelModel.UPGRADING:
				this.#errorPage(
					'An automatic upgrade is currently in progress. The backoffice will be available once the upgrade has completed.',
					undefined,
					{ headline: 'Website is Under Maintenance', hideBackButton: true },
				);
				break;

			case RuntimeLevelModel.BOOT_FAILED:
				this.#errorPage('The Umbraco server failed to boot');
				break;

			case RuntimeLevelModel.RUN: {
				// If we are on installer or upgrade page, redirect to the root since we are in the RUN state
				if (pathname === '/install' || pathname === '/upgrade') {
					history.replaceState(null, '', '/');
					break;
				}

				// Keep the current path but replace state anyway to initialize the router
				// because the router will not initialize a wildcard route by itself
				history.replaceState(null, '', location.href);
				break;
			}

			default:
				// Redirect to the error page
				this.#errorPage(`Unsupported runtime level: ${this.#serverConnection?.getStatus()}`);
		}
	}

	#isAuthorizedGuard(): Guard {
		return () => this.#authController.isAuthorized() ?? false;
	}

	#errorPage(errorMsg: string, error?: unknown, options?: { headline?: string; hideBackButton?: boolean }) {
		// Redirect to the error page
		this._routes = [
			{
				path: '**',
				component: () => import('./app-error.element.js'),
				setup: (component) => {
					(component as UmbAppErrorElement).errorMessage = errorMsg;
					(component as UmbAppErrorElement).error = error;
					if (options?.headline) (component as UmbAppErrorElement).errorHeadline = options.headline;
					if (options?.hideBackButton) (component as UmbAppErrorElement).hideBackButton = true;
				},
			},
		];

		// Re-render the router
		this.requestUpdate();
	}

	override render() {
		return html`<umb-router-slot id="router-slot" .routes=${this._routes}></umb-router-slot>`;
	}

	static override styles = css`
		:host {
			overflow: hidden;
			min-width: 920px;
		}

		:host,
		#router-slot {
			display: block;
			width: 100%;
			height: 100vh;
		}
	`;
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-app': UmbAppElement;
	}
}
