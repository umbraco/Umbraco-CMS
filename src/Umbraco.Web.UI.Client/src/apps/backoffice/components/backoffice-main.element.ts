import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbBackofficeContext, UMB_BACKOFFICE_CONTEXT_TOKEN } from '../backoffice.context.js';
import { UmbSectionContext, UMB_SECTION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/section';
import type { UmbRoute, UmbRouterSlotChangeEvent } from '@umbraco-cms/backoffice/router';
import type { ManifestSection, UmbSectionExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { createExtensionElementOrFallback } from '@umbraco-cms/backoffice/extension-api';

@customElement('umb-backoffice-main')
export class UmbBackofficeMainElement extends UmbLitElement {
	@state()
	private _routes: Array<UmbRoute & { alias: string }> = [];

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
		if (this._backofficeContext) {
			this.observe(
				this._backofficeContext.allowedSections,
				(sections) => {
					this._sections = sections;
					this._createRoutes();
				},
				'observeAllowedSections'
			);
		}
	}

	private _createRoutes() {
		if (!this._sections) return;

		// TODO: Refactor this for re-use across the app where the routes are re-generated at any time.
		// TODO: remove section-routes that does not exist anymore.
		this._routes = this._sections.map((section) => {
			const existingRoute = this._routes.find((r) => r.alias === section.alias);
			if (existingRoute) {
				return existingRoute;
			} else {
				return {
					alias: section.alias,
					path: this._routePrefix + section.meta.pathname,
					component: () => createExtensionElementOrFallback(section, 'umb-section-default'),
					setup: (component) => {
						(component as UmbSectionExtensionElement).manifest = section;
					},
				};
			}
		});

		if (!this._routes.find((r) => r.path === '**')) {
			this._routes.push({
				alias: '__redirect',
				path: '**',
				redirectTo: this._routePrefix + this._sections?.[0]?.meta.pathname,
			});
		}
	}

	private _onRouteChange = (event: UmbRouterSlotChangeEvent) => {
		const currentPath = event.target.localActiveViewPath || '';
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
		return this._routes.length > 0
			? html`<umb-router-slot .routes=${this._routes} @change=${this._onRouteChange}></umb-router-slot>`
			: '';
	}

	static styles = [
		css`
			:host {
				background-color: var(--uui-color-background);
				display: block;
				height: calc(100% - 60px); // 60 => top header height
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-backoffice-main': UmbBackofficeMainElement;
	}
}
