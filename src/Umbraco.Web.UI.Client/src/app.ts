import './core/css/custom-properties.css';
import '@umbraco-ui/uui-modal';
import '@umbraco-ui/uui-modal-container';
import '@umbraco-ui/uui-modal-dialog';
import '@umbraco-ui/uui-modal-sidebar';
import '@umbraco-ui/uui-color-swatch';
import '@umbraco-ui/uui-color-swatches';
import 'router-slot';
import 'element-internals-polyfill';

// TODO: remove these imports when they are part of UUI
import type { Guard, IRoute } from 'router-slot/model';

import { UUIIconRegistryEssential } from '@umbraco-ui/uui';
import { css, html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';

import { OpenAPI, RuntimeLevel, ServerResource } from '@umbraco-cms/backend-api';
import { UmbContextProviderMixin } from '@umbraco-cms/context-api';

import './auth';

@customElement('umb-app')
export class UmbApp extends UmbContextProviderMixin(LitElement) {
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

	@property({ type: String })
	private umbracoUrl?: string;

	@state()
	private _routes: IRoute[] = [
		{
			path: 'login',
			component: () => import('./auth/login/login.element'),
		},
		{
			path: 'install',
			component: () => import('./installer/installer.element'),
		},
		{
			path: 'upgrade',
			component: () => import('./upgrader/upgrader.element'),
			guards: [this._isAuthorizedGuard('/upgrade')],
		},
		{
			path: '**',
			component: () => import('./backoffice/backoffice.element'),
			guards: [this._isAuthorizedGuard()],
		},
	];

	private _iconRegistry = new UUIIconRegistryEssential();
	private _runtimeLevel = RuntimeLevel.UNKNOWN;

	constructor() {
		super();

		this._setup();
	}

	async connectedCallback() {
		super.connectedCallback();

		OpenAPI.BASE =
			import.meta.env.VITE_UMBRACO_USE_MSW === 'on'
				? ''
				: this.umbracoUrl ?? import.meta.env.VITE_UMBRACO_API_URL ?? '';
		OpenAPI.WITH_CREDENTIALS = true;

		this.provideContext('UMBRACOBASE', OpenAPI.BASE);

		await this._setInitStatus();
		await this._registerExtensionManifestsFromServer();
		this._redirect();
	}

	private async _setup() {
		this._iconRegistry.attach(this);
	}

	private async _setInitStatus() {
		try {
			const serverStatus = await ServerResource.getServerStatus();
			if (serverStatus.serverStatus) {
				this._runtimeLevel = serverStatus.serverStatus;
			}
		} catch (error) {
			console.log(error);
		}
	}

	private _redirect() {
		switch (this._runtimeLevel) {
			case RuntimeLevel.INSTALL:
				history.replaceState(null, '', '/install');
				break;

			case RuntimeLevel.UPGRADE:
				history.replaceState(null, '', '/upgrade');
				break;

			case RuntimeLevel.RUN: {
				const pathname =
					window.location.pathname === '/install' || window.location.pathname === '/upgrade'
						? '/'
						: window.location.pathname;
				history.replaceState(null, '', pathname);
				break;
			}

			default:
				throw new Error(`Unsupported runtime level: ${this._runtimeLevel}`);
		}
	}

	private _isAuthorized(): boolean {
		return true; // TODO: Return true for now, until new login page is up and running
		//return sessionStorage.getItem('is-authenticated') === 'true';
	}

	private _isAuthorizedGuard(redirectTo?: string): Guard {
		return () => {
			if (this._isAuthorized()) {
				return true;
			}

			let returnPath = `${OpenAPI.BASE}/umbraco/login`;

			if (redirectTo) {
				returnPath += `?redirectTo=${redirectTo}`;
			}

			// Redirect user completely to login page
			location.href = returnPath;
			return false;
		};
	}

	private async _registerExtensionManifestsFromServer() {
		// TODO: Implement once manifest endpoint exists
		// const res = await getManifests({});
		// const { manifests } = res.data as unknown as { manifests: ManifestTypes[] };
		// manifests.forEach((manifest) => umbExtensionsRegistry.register(manifest));
	}

	render() {
		return html`<router-slot id="router-slot" .routes=${this._routes}></router-slot>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-app': UmbApp;
	}
}
