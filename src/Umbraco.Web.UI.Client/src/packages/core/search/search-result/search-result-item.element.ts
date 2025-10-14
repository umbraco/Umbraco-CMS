import type { UmbSearchResultItemModel } from '../types.js';
import { css, customElement, html, nothing, property, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-search-result-item')
export class UmbSearchResultItemElement extends UmbLitElement {
	@property({ type: Object })
	item?: UmbSearchResultItemModel;

	override render() {
		if (!this.item) return nothing;
		return html`
			${when(
				this.item.icon,
				(icon) => html`<umb-icon name=${icon}></umb-icon>`,
				() => html`<uui-icon name="icon-shape-hexagon"></uui-icon>`,
			)}
			<span>${this.item.name}</span>
			<div class="extra"></div>
		`;
	}

	static override styles = [
		css`
			:host {
				border-radius: var(--uui-border-radius);
				outline-offset: -3px;
				padding: var(--uui-size-space-3) var(--uui-size-space-5);

				display: flex;
				gap: var(--uui-size-space-3);
				align-items: center;

				width: 100%;

				> span {
					flex: 1;
				}
			}
		`,
	];
}

export { UmbSearchResultItemElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-search-result-item': UmbSearchResultItemElement;
	}
}
