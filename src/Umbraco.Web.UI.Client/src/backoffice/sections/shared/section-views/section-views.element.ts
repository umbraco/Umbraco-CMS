import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { EMPTY, map, of, Subscription, switchMap } from 'rxjs';
import { UmbSectionContext } from '../section.context';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import type { ManifestSectionView } from '@umbraco-cms/models';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';

@customElement('umb-section-views')
export class UmbSectionViewsElement extends UmbContextConsumerMixin(LitElement) {
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
	private _activeView?: ManifestSectionView;

	private _sectionContext?: UmbSectionContext;
	private _viewsSubscription?: Subscription;
	private _activeViewSubscription?: Subscription;

	constructor() {
		super();

		this.consumeContext('umbSectionContext', (sectionContext: UmbSectionContext) => {
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

		this._viewsSubscription?.unsubscribe();

		this._viewsSubscription = this._sectionContext?.data
			.pipe(
				switchMap((section) => {
					if (!section) return EMPTY;

					return (
						umbExtensionsRegistry
							?.extensionsOfType('sectionView')
							.pipe(
								map((views) =>
									views
										.filter((view) => view.meta.sections.includes(section.alias))
										.sort((a, b) => b.meta.weight - a.meta.weight)
								)
							) ?? of([])
					);
				})
			)
			.subscribe((views) => {
				this._views = views;
			});
	}

	private _observeActiveView() {
		this._activeViewSubscription?.unsubscribe();

		this._activeViewSubscription = this._sectionContext?.activeView.subscribe((view) => {
			this._activeView = view;
		});
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._viewsSubscription?.unsubscribe();
		this._activeViewSubscription?.unsubscribe();
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
										?active="${this._activeView?.meta?.pathname.includes(view.meta.pathname)}">
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
