import type { UmbSearchResultItemModel } from '../types.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { css, customElement, html, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

const elementName = 'umb-search-result-item';
@customElement(elementName)
export class UmbSearchResultItemElement extends UmbLitElement {
	@property({ type: Object })
	item?: UmbSearchResultItemModel;

	render() {
		if (!this.item) return nothing;

		const url = 'url/goes/here';

		return html`
			<a href="#" class="item">
				<span class="item-icon">
					${this.item.icon ? html`<umb-icon name="${this.item.icon}"></umb-icon>` : this.#renderHashTag()}
				</span>
				<span class="item-name"> ${this.item.name} ${url ? html`<span class="item-url">${url}</span>` : nothing} </span>
				<span class="item-symbol">></span>
			</a>
		`;
	}

	#renderHashTag() {
		return html`
			<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="24" height="24">
				<path fill="none" d="M0 0h24v24H0z" />
				<path
					fill="currentColor"
					d="M7.784 14l.42-4H4V8h4.415l.525-5h2.011l-.525 5h3.989l.525-5h2.011l-.525 5H20v2h-3.784l-.42 4H20v2h-4.415l-.525 5h-2.011l.525-5H9.585l-.525 5H7.049l.525-5H4v-2h3.784zm2.011 0h3.99l.42-4h-3.99l-.42 4z" />
			</svg>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				gap: 12px;
				width: 100%;
			}
			.item {
				background: var(--uui-color-surface);
				padding: var(--uui-size-space-3) var(--uui-size-space-5);
				border-radius: var(--uui-border-radius);
				color: var(--uui-color-interactive);
				display: grid;
				grid-template-columns: var(--uui-size-space-6) 1fr var(--uui-size-space-5);
				align-items: center;
				width: 100%;
			}
			.item:hover {
				background-color: var(--uui-color-surface-emphasis);
				color: var(--uui-color-interactive-emphasis);
			}
			.item:hover .item-symbol {
				font-weight: unset;
				opacity: 1;
			}
			.item-icon {
				margin-bottom: auto;
				margin-top: 5px;
			}
			.item-icon,
			.item-symbol {
				opacity: 0.4;
			}
			.item-url {
				font-size: 0.8rem;
				line-height: 1.2;
				font-weight: 100;
			}
			.item-name {
				display: flex;
				flex-direction: column;
			}
			.item-icon > * {
				height: 1rem;
				display: flex;
				width: min-content;
			}
			.item-symbol {
				font-weight: 100;
			}
			a {
				text-decoration: none;
				color: inherit;
			}
		`,
	];
}

export { UmbSearchResultItemElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbSearchResultItemElement;
	}
}
