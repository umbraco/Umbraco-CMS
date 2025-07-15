import { UMB_DICTIONARY_COLLECTION_ALIAS } from '../collection/index.js';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbTableConfig } from '@umbraco-cms/backoffice/components';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-dictionary-collection-dashboard')
export class UmbDictionaryCollectionDashboardElement extends UmbLitElement {
	@state()
	private _tableConfig: UmbTableConfig = {
		allowSelection: false,
	};

	override render() {
		return html` <umb-collection alias=${UMB_DICTIONARY_COLLECTION_ALIAS}></umb-collection>`;
	}
}

export default UmbDictionaryCollectionDashboardElement;
declare global {
	interface HTMLElementTagNameMap {
		'umb-dictionary-collection-dashboard': UmbDictionaryCollectionDashboardElement;
	}
}
