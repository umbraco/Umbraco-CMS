import { defineElement } from '@umbraco-ui/uui-base/lib/registration';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { state } from 'lit/decorators.js';
import { IRoutingInfo } from 'router-slot';
import { UmbSectionContext, UMB_SECTION_CONTEXT_TOKEN } from '../section/section.context';
import { UmbBackofficeContext, UMB_BACKOFFICE_CONTEXT_TOKEN } from './backoffice.context';
import type { ManifestSection } from '@umbraco-cms/models';
import { UmbLitElement } from '@umbraco-cms/element';
import { createExtensionElementOrFallback } from 'libs/extensions-api/create-extension-element-or-fallback.function';

@defineElement('umb-backoffice-main')
export class UmbBackofficeMain extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				background-color: var(--uui-color-background);
				display: block;
				width: 100%;
				height: 100%;
				overflow: hidden;
			}
			router-slot {
				height: 100%;
			}
		`,
	];

	@state()
	private _routes: Array<any> = [];

	@state()
	private _sections: Array<ManifestSection> = [];

	private _routePrefix = 'section/';
	private _backofficeContext?: UmbBackofficeContext;
	private _sectionContext?: UmbSectionContext;

	constructor() {
		super();

		this.consumeContext(UMB_BACKOFFICE_CONTEXT_TOKEN, (_instance) => {
			this._backofficeContext = _instance;
			this._observeBackoffice();
		});

	}

	private async _observeBackoffice() {
		if(this._backofficeContext) {
			this.observe(this._backofficeContext.getAllowedSections(), (sections) => {
				this._sections = sections;
				this._createRoutes();
			}, 'observeAllowedSections');
		}
	}

	private _createRoutes() {
		if (!this._sections) return;

		this._routes = [];
		this._routes = this._sections.map((section) => {
			return {
				path: this._routePrefix + section.meta.pathname,
				component: () => createExtensionElementOrFallback(section, 'umb-section'),
				setup: this._onRouteSetup,
				// TODO: sometimes we can end up in a state where this callback doesn't get called. It could look like a bug in the router-slot.
				// Niels: Could this be because _backofficeContext is not available at that state?
			};
		});

		this._routes.push({
			path: '**',
			redirectTo: this._routePrefix + this._sections?.[0]?.meta.pathname,
		});
	}

	private _onRouteSetup = (_component: HTMLElement, info: IRoutingInfo) => {
		const currentPath = info.match.route.path;
		const section = this._sections.find((s) => this._routePrefix + s.meta.pathname === currentPath);
		if (!section) return;
		this._backofficeContext?.setActiveSectionAlias(section.alias);
		this._provideSectionContext(section);
	};

	private _provideSectionContext(section: ManifestSection) {
		if (!this._sectionContext) {
			this._sectionContext = new UmbSectionContext(section);
			this.provideContext(UMB_SECTION_CONTEXT_TOKEN, this._sectionContext);
		} else {
			this._sectionContext.setManifest(section);
		}
	}

	render() {
		return html`<router-slot .routes=${this._routes}></router-slot>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-backoffice-main': UmbBackofficeMain;
	}
}
