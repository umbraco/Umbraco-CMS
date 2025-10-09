import { UmbNewsDashboardRepository } from './repository/index.js';
import { css, customElement, html, repeat, state, unsafeHTML, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { NewsDashboardItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

import './umbraco-welcome-dashboard.element.js';

@customElement('umb-umbraco-news-dashboard')
export class UmbUmbracoNewsDashboardElement extends UmbLitElement {
	#repository = new UmbNewsDashboardRepository(this);

	@state()
	private _items: Array<NewsDashboardItemResponseModel> = [];

	override connectedCallback() {
		super.connectedCallback();

		this.#getUmbracoNews();
	}

	async #getUmbracoNews() {
		const { items } = await this.#repository.getNewsDashboard();
		this._items = items;
	}

	override render() {
		console.log('render', this._items);
		if (!this._items) return html`<umb-umbraco-welcome-dashboard></umb-umbraco-welcome-dashboard>`;
		return html`
			<uui-box headline="Umbraco News" class="uui-text">
				${repeat(
					this._items,
					(_, index) => index,
					(item) => this.#renderItem(item),
				)}
			</uui-box>
		`;
	}

	#renderItem(item: NewsDashboardItemResponseModel) {
		const tagColor = item.priority === 'High' ? 'danger' : item.priority === 'Medium' ? 'warning' : 'default';
		return html`
			<article>
				<h3>
					${when(
						item.url,
						(url) => html`<a href=${url} target="_blank">${item.header}</a>`,
						() => html`${item.header}`,
					)}
				</h3>
				<uui-badge color=${tagColor}>${item.priority}</uui-badge>
				${when(item.imageUrl, (src) => html`<img src=${src} alt=${item.imageAltText ?? ''} />`)}
				<div>${unsafeHTML(item.body)}</div>
				${when(
					item.url,
					(url) => html`
						<uui-button class="link" look="primary" href=${url} target="_blank">
							<uui-icon name="icon-out" slot="extra"></uui-icon>
							<span>${item.buttonText}</span>
						</uui-button>
					`,
				)}
			</article>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				padding: var(--uui-size-layout-1);
			}

			article {
				position: relative;
				margin-bottom: var(--uui-size-layout-1);
				padding-bottom: var(--uui-size-layout-1);
				border-bottom: 1px solid var(--uui-color-border);

				img {
					max-width: 100%;
				}

				.link > uui-icon {
					margin-left: var(--uui-size-3);
				}
			}
		`,
	];
}

export default UmbUmbracoNewsDashboardElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-umbraco-news-dashboard': UmbUmbracoNewsDashboardElement;
	}
}
