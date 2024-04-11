import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';

const elementName = 'umb-search-result-item';
@customElement(elementName)
export class UmbSearchResultItemElement extends UmbLitElement {
	@property({ type: Object })
	item: any;

	render() {
		return html`Helloasasdas`;
	}
}

export { UmbSearchResultItemElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbSearchResultItemElement;
	}
}
