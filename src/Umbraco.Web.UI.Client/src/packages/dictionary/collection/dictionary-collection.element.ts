import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbCollectionDefaultElement } from '@umbraco-cms/backoffice/collection';

@customElement('umb-dictionary-collection')
export class UmbDictionaryCollectionElement extends UmbCollectionDefaultElement {
	protected override renderToolbar() {
		return html`
			<umb-collection-toolbar slot="header">
				<umb-collection-filter-field></umb-collection-filter-field>
			</umb-collection-toolbar>
		`;
	}
}

export { UmbDictionaryCollectionElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-dictionary-collection': UmbDictionaryCollectionElement;
	}
}
