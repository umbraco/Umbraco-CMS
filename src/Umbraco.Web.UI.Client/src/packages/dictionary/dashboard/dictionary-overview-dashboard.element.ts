import { UMB_DICTIONARY_COLLECTION_ALIAS } from '../collection/index.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbTableConfig } from '@umbraco-cms/backoffice/components';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-dictionary-collection-dashboard')
export class UmbDictionaryCollectionDashboardElement extends UmbLitElement {
	@state()
	private _tableConfig: UmbTableConfig = {
		allowSelection: false,
	};

	render() {
		return html` <umb-collection alias=${UMB_DICTIONARY_COLLECTION_ALIAS}></umb-collection>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbDictionaryCollectionDashboardElement;
declare global {
	interface HTMLElementTagNameMap {
		'umb-dictionary-collection-dashboard': UmbDictionaryCollectionDashboardElement;
	}
}
