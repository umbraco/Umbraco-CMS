import '@umbraco-ui/uui-css/dist/uui-css.css';
import './core/css/custom-properties.css';

import 'element-internals-polyfill';

import './core/router/router-slot.element';
import './core/router/variant-router-slot.element';
import './core/notification/layouts/default';
import './core/modal/modal-element.element';

import { UUIIconRegistryEssential } from '@umbraco-ui/uui';
import { css, html } from 'lit';
import { customElement, property } from 'lit/decorators.js';

import { UmbIconStore } from './core/stores/icon/icon.store';
import type { Guard, IRoute } from '@umbraco-cms/backoffice/router';
import { pathWithoutBasePath, queryString } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { OpenAPI, RuntimeLevelModel, ServerResource } from '@umbraco-cms/backoffice/backend-api';
import { contextData, umbDebugContextEventType } from '@umbraco-cms/backoffice/context-api';

@customElement('umb-app')
export class UmbAppElement extends UmbLitElement {
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

	private _routes: IRoute[] = [
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

	connectedCallback() {
		super.connectedCallback();

		OpenAPI.BASE =
			import.meta.env.VITE_UMBRACO_USE_MSW === 'on'
				? ''
				: this.umbracoUrl ?? import.meta.env.VITE_UMBRACO_API_URL ?? '';
		OpenAPI.WITH_CREDENTIALS = true;

		this.provideContext('UMBRACOBASE', OpenAPI.BASE);

		this._setInitStatus();

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

	private async _setup() {
		await this._setInitStatus();
		this._iconRegistry.attach(this);
	}

	private async _setInitStatus() {
		const { data } = await tryExecuteAndNotify(this, ServerResource.getServerStatus());
		this._runtimeLevel = data?.serverStatus ?? RuntimeLevelModel.UNKNOWN;
		this._redirect();
	}

	private _redirect() {
		switch (this._runtimeLevel) {
			case RuntimeLevelModel.INSTALL:
				history.replaceState(null, '', 'install');
				break;

			case RuntimeLevelModel.UPGRADE:
				history.replaceState(null, '', 'upgrade');
				break;

			case RuntimeLevelModel.RUN: {
				const pathname = pathWithoutBasePath();

				// If we are on the installer or upgrade page, redirect to the root
				// but if not, keep the current path but replace state anyway to initialize the router
				let finalPath = pathname === 'install' || pathname === 'upgrade' ? '/' : pathname;

				const qs = queryString();

				if (qs) {
					finalPath += qs;
				}

				if (location.hash) {
					finalPath += `#${location.hash}`;
				}

				history.replaceState(null, '', finalPath);
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
		'umb-app': UmbAppElement;
	}
}
