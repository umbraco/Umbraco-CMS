import { UMB_BACKOFFICE_CONTEXT } from '../backoffice.context.js';
import type { UmbBackofficeContext } from '../backoffice.context.js';
import { createExtensionApi, createExtensionElement } from '@umbraco-cms/backoffice/extension-api';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbDefaultSectionContext, UMB_SECTION_PATH_PATTERN } from '@umbraco-cms/backoffice/section';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { ManifestSection, UmbSectionElement } from '@umbraco-cms/backoffice/section';
import type { PageComponent, UmbRoute } from '@umbraco-cms/backoffice/router';

@customElement('umb-backoffice-main')
export class UmbBackofficeMainElement extends UmbLitElement {
	@state()
	private _routes: Array<UmbRoute> = [];

	@state()
	private _sections: Array<ManifestSection> = [];

	#backofficeContext?: UmbBackofficeContext;

	constructor() {
		super();

		this.consumeContext(UMB_BACKOFFICE_CONTEXT, (_instance) => {
			this.#backofficeContext = _instance;
			this._observeBackoffice();
		});
	}

	private async _observeBackoffice() {
		if (this.#backofficeContext) {
			this.observe(
				this.#backofficeContext.allowedSections,
				(sections) => {
					this._sections = sections.map((section) => section.manifest).filter((section) => section !== undefined);
					this.#createRoutes();
				},
				'observeAllowedSections',
			);
		}
	}

	#createRoutes() {
		if (!this._sections) return;

		// TODO: Refactor this for re-use across the app where the routes are re-generated at any time.
		const newRoutes: Array<UmbRoute> = this._sections.map((manifest) => {
			return {
				path: UMB_SECTION_PATH_PATTERN.generateLocal({ sectionName: manifest.meta.pathname }),
				component: () => createExtensionElement(manifest, 'umb-section-default'),
				setup: async (component: PageComponent) => {
					if (component) {
						const element = component as UmbSectionElement;

						element.manifest = manifest;

						const api = await createExtensionApi(element, manifest, [], UmbDefaultSectionContext);
						if (api) {
							api.manifest = manifest;
						}
					}

					this.#backofficeContext?.setActiveSectionAlias(manifest.alias);
				},
			};
		});

		if (newRoutes.length > 0) {
			newRoutes.push({
				path: '',
				pathMatch: 'full',
				awaitStability: true,
				redirectTo: newRoutes[0].path,
			});

			newRoutes.push({
				path: `**`,
				component: async () => (await import('@umbraco-cms/backoffice/router')).UmbRouteNotFoundElement,
			});
		}

		this._routes = newRoutes;
	}

	override render() {
		if (!this._routes.length) return;
		return html`<umb-router-slot .routes=${this._routes}></umb-router-slot>`;
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
