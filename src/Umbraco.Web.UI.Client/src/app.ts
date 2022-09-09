import './css/custom-properties.css';
import 'router-slot';

// TODO: remove these imports when they are part of UUI
import '@umbraco-ui/uui-modal';
import '@umbraco-ui/uui-modal-sidebar';
import '@umbraco-ui/uui-modal-container';
import '@umbraco-ui/uui-modal-dialog';

import { UUIIconRegistryEssential } from '@umbraco-ui/uui';
import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';

import { getManifests, getServerStatus } from './core/api/fetcher';
import { UmbContextProviderMixin } from './core/context';
import { UmbExtensionRegistry } from './core/extension';
import { internalManifests } from './temp-internal-manifests';

import type { ServerStatus } from './core/models';
import type { Guard, IRoute } from 'router-slot/model';

// TODO Remove these imports when they are a part of UUI
import '@umbraco-ui/uui-modal';
import '@umbraco-ui/uui-modal-sidebar';
import '@umbraco-ui/uui-modal-container';
import '@umbraco-ui/uui-modal-dialog';

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

	private _extensionRegistry = new UmbExtensionRegistry();
	private _iconRegistry = new UUIIconRegistryEssential();
	private _serverStatus: ServerStatus = 'running';

	constructor() {
		super();
		this._setup();
	}

	private async _setup() {
		this._iconRegistry.attach(this);
		this.provideContext('umbExtensionRegistry', this._extensionRegistry);

		await this._registerExtensionManifestsFromServer();
		await this._registerInternalManifests();
		await this._setInitStatus();
		this._redirect();
	}

	private async _setInitStatus() {
		try {
			const { data } = await getServerStatus({});
			this._serverStatus = data.serverStatus;
		} catch (error) {
			console.log(error);
		}
	}

	private _redirect() {
		switch (this._serverStatus) {
			case 'must-install':
				history.replaceState(null, '', '/install');
				break;

			case 'must-upgrade':
				history.replaceState(null, '', '/upgrade');
				break;

			case 'running': {
				const pathname =
					window.location.pathname === '/install' || window.location.pathname === '/upgrade'
						? '/'
						: window.location.pathname;
				history.replaceState(null, '', pathname);
				break;
			}
		}
	}

	private _isAuthorized(): boolean {
		return sessionStorage.getItem('is-authenticated') === 'true';
	}

	private _isAuthorizedGuard(redirectTo?: string): Guard {
		return () => {
			if (this._isAuthorized()) {
				return true;
			}

			let returnPath = '/login';

			if (redirectTo) {
				returnPath += `?redirectTo=${redirectTo}`;
			}

			history.replaceState(null, '', returnPath);
			return false;
		};
	}

	private async _registerExtensionManifestsFromServer() {
		const res = await getManifests({});
		const { manifests } = res.data;
		manifests.forEach((manifest) => this._extensionRegistry.register(manifest));
	}

	private async _registerInternalManifests() {
		// TODO: where do we get these from?
		internalManifests.forEach((manifest) => this._extensionRegistry.register(manifest));
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
