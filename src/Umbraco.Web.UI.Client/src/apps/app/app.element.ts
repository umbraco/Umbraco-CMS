import type { UmbAppErrorElement } from './app-error.element.js';
import { UMB_AUTH, UmbAuthFlow, UmbAuthStore } from '@umbraco-cms/backoffice/auth';
import { UMB_APP, UmbAppContext } from '@umbraco-cms/backoffice/context';
import { css, html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UUIIconRegistryEssential } from '@umbraco-cms/backoffice/external/uui';
import { UmbIconRegistry } from '@umbraco-cms/backoffice/icon';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { Guard, UmbRoute } from '@umbraco-cms/backoffice/router';
import { pathWithoutBasePath } from '@umbraco-cms/backoffice/router';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { OpenAPI, RuntimeLevelModel, ServerResource } from '@umbraco-cms/backoffice/backend-api';
import { contextData, umbDebugContextEventType } from '@umbraco-cms/backoffice/context-api';

@customElement('umb-app')
export class UmbAppElement extends UmbLitElement {
	/**
	 * The base URL of the configured Umbraco server.
	 *
	 * @attr
	 * @remarks This is the base URL of the Umbraco server, not the base URL of the backoffice.
	 */
	@property({ type: String })
	serverUrl = window.location.origin;

	/**
	 * The base path of the backoffice.
	 *
	 * @attr
	 */
	@property({ type: String })
	// TODO: get from server config
	backofficePath = '/umbraco';

	/**
	 * Bypass authentication.
	 * @type {boolean}
	 */
	// TODO: this might not be the right solution
	@property({ type: Boolean })
	bypassAuth = false;

	private _routes: UmbRoute[] = [
		{
			path: 'install',
			component: () => import('../installer/installer.element.js'),
		},
		{
			path: 'upgrade',
			component: () => import('../upgrader/upgrader.element.js'),
			guards: [this.#isAuthorizedGuard()],
		},
		{
			path: '**',
			component: () => import('../backoffice/backoffice.element.js'),
			guards: [this.#isAuthorizedGuard()],
		},
	];

	#authFlow?: UmbAuthFlow;
	#umbIconRegistry = new UmbIconRegistry();
	#uuiIconRegistry = new UUIIconRegistryEssential();
	#runtimeLevel = RuntimeLevelModel.UNKNOWN;

	constructor() {
		super();

		this.#umbIconRegistry.attach(this);
		this.#uuiIconRegistry.attach(this);
	}

	connectedCallback(): void {
		super.connectedCallback();
		this.#setup();
	}

	async #setup() {
		if (this.serverUrl === undefined) throw new Error('No serverUrl provided');

		OpenAPI.BASE = this.serverUrl;
		const redirectUrl = `${window.location.origin}${this.backofficePath}`;

		this.#authFlow = new UmbAuthFlow(this.serverUrl, redirectUrl);

		const authStore = new UmbAuthStore(this, this.#authFlow);

		this.provideContext(UMB_AUTH, authStore);

		this.provideContext(UMB_APP, new UmbAppContext({ backofficePath: this.backofficePath, serverUrl: this.serverUrl }));

		// Try to initialise the auth flow and get the runtime status
		try {
			// Get the current runtime level
			await this.#setInitStatus();

			if (this.bypassAuth === false) {
				// Get service configuration from authentication server
				await this.#authFlow.setInitialState();

				// Instruct all requests to use the auth flow to get and use the access_token for all subsequent requests
				OpenAPI.TOKEN = () => this.#authFlow!.performWithFreshTokens();
				OpenAPI.WITH_CREDENTIALS = true;
			}

			authStore.isLoggedIn.next(true);

			// Initialise the router
			this.#redirect();
		} catch (error) {
			// If the auth flow fails, there is most likely something wrong with the connection to the backend server
			// and we should redirect to the error page
			let errorMsg =
				'An error occured while trying to initialise the connection to the Umbraco server (check console for details)';

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

		// TODO: wrap all debugging logic in a separate class. Maybe this could be part of the context-api? When we create a new root, we could attach the debugger to it?
		// Listen for the debug event from the <umb-debug> component
		this.addEventListener(umbDebugContextEventType, (event: any) => {
			// Once we got to the outter most component <umb-app>
			// we can send the event containing all the contexts
			// we have collected whilst coming up through the DOM
			// and pass it back down to the callback in
			// the <umb-debug> component that originally fired the event
			if (event.callback) {
				event.callback(event.instances);
			}

			// Massage the data into a simplier format
			// Why? Can't send contexts data directly - browser seems to not serialize it and says its null
			// But a simple object works fine for browser extension to consume
			const data = {
				contexts: contextData(event.instances),
			};

			// Emit this new event for the browser extension to listen for
			this.dispatchEvent(new CustomEvent('umb:debug-contexts:data', { detail: data, bubbles: true }));
		});
	}

	async #setInitStatus() {
		const { data, error } = await tryExecute(ServerResource.getServerStatus());
		if (error) {
			throw error;
		}
		this.#runtimeLevel = data?.serverStatus ?? RuntimeLevelModel.UNKNOWN;
	}

	#redirect() {
		switch (this.#runtimeLevel) {
			case RuntimeLevelModel.INSTALL:
				history.replaceState(null, '', 'install');
				break;

			case RuntimeLevelModel.UPGRADE:
				history.replaceState(null, '', 'upgrade');
				break;

			case RuntimeLevelModel.BOOT_FAILED:
				this.#errorPage('The Umbraco server failed to boot');
				break;

			case RuntimeLevelModel.RUN: {
				const pathname = pathWithoutBasePath({ start: true, end: false });

				// If we are on the installer or upgrade page, redirect to the root
				// but if not, keep the current path but replace state anyway to initialize the router
				let currentRoute = location.href;
				const savedRoute = sessionStorage.getItem('umb:auth:redirect');
				if (savedRoute) {
					sessionStorage.removeItem('umb:auth:redirect');
					currentRoute = savedRoute;
				}
				const finalPath = pathname === '/install' || pathname === '/upgrade' ? '/' : currentRoute;

				history.replaceState(null, '', finalPath);
				break;
			}

			default:
				// Redirect to the error page
				this.#errorPage(`Unsupported runtime level: ${this.#runtimeLevel}`);
		}
	}

	#isAuthorized(): boolean {
		if (!this.#authFlow) return false;
		return this.bypassAuth ? true : this.#authFlow.loggedIn();
	}

	#isAuthorizedGuard(): Guard {
		return () => {
			if (this.#isAuthorized()) {
				return true;
			}

			// Save location.href so we can redirect to it after login
			window.sessionStorage.setItem('umb:auth:redirect', location.href);

			// Make a request to the auth server to start the auth flow
			this.#authFlow!.makeAuthorizationRequest();

			// Return false to prevent the route from being rendered
			return false;
		};
	}

	#errorPage(errorMsg: string, error?: unknown) {
		// Redirect to the error page
		this._routes = [
			{
				path: '**',
				component: () => import('./app-error.element.js'),
				setup: (component) => {
					(component as UmbAppErrorElement).errorMessage = errorMsg;
					(component as UmbAppErrorElement).error = error;
				},
			},
		];

		// Re-render the router
		this.requestUpdate();
	}

	render() {
		return html`<umb-router-slot id="router-slot" .routes=${this._routes}></umb-router-slot>`;
	}

	static styles = css`
		:host {
			overflow: hidden;
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
