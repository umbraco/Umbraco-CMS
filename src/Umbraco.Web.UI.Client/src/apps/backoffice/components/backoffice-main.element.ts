import { UmbBackofficeContext, UMB_BACKOFFICE_CONTEXT_TOKEN } from '../backoffice.context.js';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbSectionContext, UMB_SECTION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/section';
import type { UmbRoute, UmbRouterSlotChangeEvent } from '@umbraco-cms/backoffice/router';
import type { ManifestSection, UmbSectionExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import {
	UmbExtensionManifestController,
	createExtensionElementOrFallback,
} from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-backoffice-main')
export class UmbBackofficeMainElement extends UmbLitElement {
	@state()
	private _routes: Array<UmbRoute & { alias: string }> = [];

	@state()
	private _sections: Array<UmbExtensionManifestController<ManifestSection>> = [];

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
		const oldValue = this._routes;

		// TODO: Refactor this for re-use across the app where the routes are re-generated at any time.
		// TODO: remove section-routes that does not exist anymore.
		this._routes = this._sections.map((section) => {
			const existingRoute = this._routes.find((r) => r.alias === section.alias);
			if (existingRoute) {
				return existingRoute;
			} else {
				return {
					alias: section.alias,
					path: this._routePrefix + (section.manifest as any).meta.pathname,
					component: () => createExtensionElementOrFallback(section.manifest, 'umb-section-default'),
					setup: (component) => {
						(component as UmbSectionExtensionElement).manifest = section.manifest as any;
					},
				};
			}
		});

		if (this._sections.length > 0) {
			this._routes.push({
				alias: '__redirect',
				path: '/',
				redirectTo: 'section/content',
			});
		}

		this.requestUpdate('_routes', oldValue);
	}

	private _onRouteChange = (event: UmbRouterSlotChangeEvent) => {
		const currentPath = event.target.localActiveViewPath || '';
		const section = this._sections.find((s) => this._routePrefix + (s.manifest as any).meta.pathname === currentPath);
		if (!section) return;
		this._backofficeContext?.setActiveSectionAlias(section.alias);
		this._provideSectionContext(section.manifest as any);
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
