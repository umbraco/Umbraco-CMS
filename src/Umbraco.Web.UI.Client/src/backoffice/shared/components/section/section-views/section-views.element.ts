import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { map, of } from 'rxjs';
import { UmbSectionContext, UMB_SECTION_CONTEXT_TOKEN } from '../section.context';
import type { ManifestSectionView } from '@umbraco-cms/models';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-api';
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

	@property()
	private routerPath?: string;

	@property()
	private activePath?: string;

	private _sectionContext?: UmbSectionContext;
	private _extensionsObserver?: UmbObserverController<ManifestSectionView[]>;

	constructor() {
		super();

		this.consumeContext(UMB_SECTION_CONTEXT_TOKEN, (sectionContext) => {
			this._sectionContext = sectionContext;
			this._observeViews();
		});
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
				}
			);
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
										href="${this.routerPath}/view/${view.meta.pathname}"
										?active="${this.activePath === 'view/' + view.meta.pathname}">
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
