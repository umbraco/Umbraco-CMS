import type { UmbBackofficeContext } from '../backoffice.context.js';
import { UMB_BACKOFFICE_CONTEXT } from '../backoffice.context.js';
import { css, html, customElement, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbSectionContext, UMB_SECTION_PATH_PATTERN } from '@umbraco-cms/backoffice/section';
import type { PageComponent, UmbRoute, UmbRouterSlotChangeEvent } from '@umbraco-cms/backoffice/router';
import type { ManifestSection, UmbSectionElement } from '@umbraco-cms/backoffice/section';
import type { UmbExtensionManifestInitializer } from '@umbraco-cms/backoffice/extension-api';
import { createExtensionElement } from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-backoffice-main')
export class UmbBackofficeMainElement extends UmbLitElement {
	@state()
	private _routes: Array<UmbRoute> = [];

	@state()
	private _sections: Array<UmbExtensionManifestInitializer<ManifestSection>> = [];

	private _backofficeContext?: UmbBackofficeContext;
	private _sectionContext?: UmbSectionContext;

	constructor() {
		super();

		this.consumeContext(UMB_BACKOFFICE_CONTEXT, (_instance) => {
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
				'observeAllowedSections',
			);
		}
	}

	private _createRoutes() {
		if (!this._sections) return;

		// TODO: Refactor this for re-use across the app where the routes are re-generated at any time.
		const newRoutes = this._sections
			.filter((x) => x.manifest)
			.map((section) => {
				const existingRoute = this._routes.find(
					(r) => r.path === UMB_SECTION_PATH_PATTERN.generateLocal({ sectionName: section.manifest!.meta.pathname }),
				);
				if (existingRoute) {
					return existingRoute;
				} else {
					return {
						//alias: section.alias,
						path: UMB_SECTION_PATH_PATTERN.generateLocal({ sectionName: section.manifest!.meta.pathname }),
						component: () => createExtensionElement(section.manifest!, 'umb-section-default'),
						setup: (component: PageComponent) => {
							(component as UmbSectionElement).manifest = section.manifest;
						},
					};
				}
			});

		if (newRoutes.length > 0) {
			newRoutes.push({
				...newRoutes[0],
				path: ``,
			});

			newRoutes.push({
				path: `**`,
				component: async () => (await import('@umbraco-cms/backoffice/router')).UmbRouteNotFoundElement,
			});
		}

		this._routes = newRoutes;
	}

	private _onRouteChange = async (event: UmbRouterSlotChangeEvent) => {
		const currentPath = event.target.localActiveViewPath || '';
		const initializer = this._sections.find(
			(s) => UMB_SECTION_PATH_PATTERN.generateLocal({ sectionName: s.manifest!.meta.pathname }) === currentPath,
		);
		if (!initializer) return;
		await initializer.asPromise();
		if (initializer.manifest) {
			this._backofficeContext?.setActiveSectionAlias(initializer.alias);
			this._provideSectionContext(initializer.manifest);
		}
	};

	private _provideSectionContext(sectionManifest: ManifestSection) {
		if (!this._sectionContext) {
			this._sectionContext = new UmbSectionContext(this);
		}

		this._sectionContext.setManifest(sectionManifest);
	}

	override render() {
		return this._routes.length > 0
			? html`<umb-router-slot .routes=${this._routes} @change=${this._onRouteChange}></umb-router-slot>`
			: nothing;
	}

	static override styles = [
		css`
			:host {
				display: block;
				background-color: var(--uui-color-background);
				width: 100%;
				height: calc(
					100% - 60px
				); /* 60 => top header height, TODO: Make sure this comes from somewhere so it is maintainable and eventually responsive. */
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-backoffice-main': UmbBackofficeMainElement;
	}
}
