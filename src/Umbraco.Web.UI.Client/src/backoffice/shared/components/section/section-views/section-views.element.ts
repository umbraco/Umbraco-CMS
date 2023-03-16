import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { map, of } from 'rxjs';
import { UmbRouterSlotChangeEvent, UmbRouterSlotInitEvent } from '@umbraco-cms/router';
import { UmbSectionContext, UMB_SECTION_CONTEXT_TOKEN } from '../section.context';
import type { ManifestSectionView } from '@umbraco-cms/models';
import { createExtensionElement, umbExtensionsRegistry } from '@umbraco-cms/extensions-api';
import { UmbLitElement } from '@umbraco-cms/element';
import { UmbObserverController } from '@umbraco-cms/observable-api';

@customElement('umb-section-views')
export class UmbSectionViewsElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			#header {
				background-color: var(--uui-color-surface);
				border-bottom: 1px solid var(--uui-color-divider-standalone);
			}

			uui-tab-group {
				justify-content: flex-end;
				--uui-tab-divider: var(--uui-color-divider-standalone);
			}

			uui-tab-group uui-tab:first-child {
				border-left: 1px solid var(--uui-color-divider-standalone);
			}
		`,
	];

	@property({ type: String, attribute: 'section-alias' })
	public sectionAlias?: string;

	@state()
	private _views: Array<ManifestSectionView> = [];

	@state()
	private _routerPath?: string;

	@state()
	private _activePath?: string;

	@state()
	private _routes: Array<any> = [];

	private _sectionContext?: UmbSectionContext;
	private _extensionsObserver?: UmbObserverController<ManifestSectionView[]>;

	constructor() {
		super();

		this.consumeContext(UMB_SECTION_CONTEXT_TOKEN, (sectionContext) => {
			this._sectionContext = sectionContext;
			this._observeViews();
		});
	}

	async #createRoutes(viewManifests: Array<ManifestSectionView>) {
		if (!viewManifests) return;
		const routes = viewManifests.map((manifest) => {
			return {
				path: manifest.meta.pathname,
				component: () => createExtensionElement(manifest),
			};
		});

		this._routes = [...routes, { path: '**', redirectTo: routes[0].path }];
	}

	private _observeViews() {
		if (!this._sectionContext) return;

		this.observe(
			this._sectionContext.alias,
			(sectionAlias) => {
				this._observeExtensions(sectionAlias);
			},
			'viewsObserver'
		);
	}
	private _observeExtensions(sectionAlias?: string) {
		this._extensionsObserver?.destroy();
		if (sectionAlias) {
			this._extensionsObserver = this.observe(
				umbExtensionsRegistry
					?.extensionsOfType('sectionView')
					.pipe(map((views) => views.filter((view) => view.conditions.sections.includes(sectionAlias)))) ?? of([]),
				(views) => {
					this._views = views;
					this.#createRoutes(views);
				}
			);
		}
	}

	render() {
		return html`
			${this._views.length > 0
				? html`
						<div id="header">${this.#renderTabs()}</div>
						<umb-router-slot
							.routes=${this._routes}
							@init=${(event: UmbRouterSlotInitEvent) => {
								this._routerPath = event.target.absoluteRouterPath;
							}}
							@change=${(event: UmbRouterSlotChangeEvent) => {
								this._activePath = event.target.localActiveViewPath;
							}}>
						</umb-router-slot>
				  `
				: nothing}
		`;
	}

	#renderTabs() {
		return html`
			<uui-tab-group>
				${this._views.map(
					(view: ManifestSectionView) => html`
						<uui-tab
							.label="${view.meta.label || view.name}"
							href="${this._routerPath}/${view.meta.pathname}"
							?active="${this._activePath === view.meta.pathname}">
							<uui-icon slot="icon" name=${view.meta.icon}></uui-icon>
							${view.meta.label || view.name}
						</uui-tab>
					`
				)}
			</uui-tab-group>
		`;
	}
}

export default UmbSectionViewsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-views': UmbSectionViewsElement;
	}
}
