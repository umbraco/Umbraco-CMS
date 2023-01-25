import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { map, of } from 'rxjs';
import { UmbSectionContext, UMB_SECTION_CONTEXT_TOKEN } from '../section.context';
import type { ManifestSectionView } from '@umbraco-cms/models';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
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

	@state()
	private _views: Array<ManifestSectionView> = [];

	@state()
	private _routerFolder = '';

	@state()
	private _activeViewPathname?: string;

	private _sectionContext?: UmbSectionContext;
	private _extensionsObserver?: UmbObserverController;

	constructor() {
		super();

		this.consumeContext(UMB_SECTION_CONTEXT_TOKEN, (sectionContext) => {
			this._sectionContext = sectionContext;
			this._observeViews();
			this._observeActiveView();
		});
	}

	connectedCallback(): void {
		super.connectedCallback();
		/* TODO: find a way to construct absolute urls */
		this._routerFolder = window.location.pathname.split('/view')[0];
	}

	private _observeViews() {
		if (!this._sectionContext) return;

		this.observe(this._sectionContext.alias, (sectionAlias) => {
			this._observeExtensions(sectionAlias);
		}, 'viewsObserver')
	}
	private _observeExtensions(sectionAlias?: string) {
		this._extensionsObserver?.destroy();
		if(sectionAlias) {
			this._extensionsObserver = this.observe(
					umbExtensionsRegistry?.extensionsOfType('sectionView').pipe(
						map((views) =>
							views
								.filter((view) => view.meta.sections.includes(sectionAlias))
								.sort((a, b) => b.meta.weight - a.meta.weight)
						)
					) ?? of([])
				,
					(views) => {
						this._views = views;
					}
			);
		}
	}

	private _observeActiveView() {
		if(this._sectionContext) {
			this.observe(this._sectionContext?.activeViewPathname, (pathname) => {
				this._activeViewPathname = pathname;
			}, 'activeViewPathnameObserver');
		}
	}

	render() {
		return html` ${this._views.length > 0 ? html` <div id="header">${this._renderViews()}</div> ` : nothing} `;
	}

	private _renderViews() {
		return html`
			${this._views?.length > 0
				? html`
						<uui-tab-group>
							${this._views.map(
								(view: ManifestSectionView) => html`
									<uui-tab
										.label="${view.meta.label || view.name}"
										href="${this._routerFolder}/view/${view.meta.pathname}"
										?active="${this._activeViewPathname?.includes(view.meta.pathname)}">
										<uui-icon slot="icon" name=${view.meta.icon}></uui-icon>
										${view.meta.label || view.name}
									</uui-tab>
								`
							)}
						</uui-tab-group>
				  `
				: nothing}
		`;
	}
}

export default UmbSectionViewsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-views': UmbSectionViewsElement;
	}
}
