import '@umbraco-ui/uui-css/dist/uui-css.css';
import '@umbraco-cms/css';

// TODO: remove these imports when they are part of UUI
import '@umbraco-ui/uui-modal';
import '@umbraco-ui/uui-modal-container';
import '@umbraco-ui/uui-modal-dialog';
import '@umbraco-ui/uui-modal-sidebar';
import 'element-internals-polyfill';

import './core/router/router-slot.element';
import './core/notification/layouts/default';

import { UUIIconRegistryEssential } from '@umbraco-ui/uui';
import { css, html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';

import type { Guard, IRoute } from '@umbraco-cms/router';
import { UmbLitElement } from '@umbraco-cms/element';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';
import { OpenAPI, RuntimeLevelModel, ServerResource } from '@umbraco-cms/backend-api';
import { UmbIconStore } from '@umbraco-cms/store';
import { umbDebugContextEventType } from '@umbraco-cms/context-api';

@customElement('umb-app')
export class UmbApp extends UmbLitElement {
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
	private _routes: IRoute<any>[] = [
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

	private _umbIconRegistry = new UmbIconStore();

	private _iconRegistry = new UUIIconRegistryEssential();
	private _runtimeLevel = RuntimeLevelModel.UNKNOWN;

	constructor() {
		super();

		this._umbIconRegistry.attach(this);

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
		this._redirect();

		// Listen for the debug event from the <umb-debug> component
		this.addEventListener(umbDebugContextEventType, (event: any) => {
			// Once we got to the outter most component <umb-app>
			// we can send the event containing all the contexts
			// we have collected whilst coming up through the DOM
			// and pass it back down to the callback in
			// the <umb-debug> component that originally fired the event
			event.callback(event.instances);
		});
	}

	private async _setup() {
		this._iconRegistry.attach(this);
	}

	private async _setInitStatus() {
		const { data } = await tryExecuteAndNotify(this, ServerResource.getServerStatus());
		this._runtimeLevel = data?.serverStatus ?? RuntimeLevelModel.UNKNOWN;
	}

	private _redirect() {
		switch (this._runtimeLevel) {
			case RuntimeLevelModel.INSTALL:
				history.replaceState(null, '', '/install');
				break;

			case RuntimeLevelModel.UPGRADE:
				history.replaceState(null, '', '/upgrade');
				break;

			case RuntimeLevelModel.RUN: {
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

	render() {
		return html`<umb-router-slot id="router-slot" .routes=${this._routes}></umb-router-slot>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-app': UmbApp;
	}
}
