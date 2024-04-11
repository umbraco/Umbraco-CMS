import type { UmbSearchResultItemModel } from '../types.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { css, customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

const elementName = 'umb-search-result-item';
@customElement(elementName)
export class UmbSearchResultItemElement extends UmbLitElement {
	@property({ type: Object })
	item?: UmbSearchResultItemModel;

	render() {
		return html`
			<div>ICON</div>
			<div>${this.item?.name}</div>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				gap: 12px;
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
