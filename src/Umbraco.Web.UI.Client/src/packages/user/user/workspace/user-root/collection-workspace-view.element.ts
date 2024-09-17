import { UMB_USER_COLLECTION_ALIAS } from '../../collection/index.js';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const elementName = 'umb-collection-workspace-view';
@customElement(elementName)
export class UmbCollectionWorkspaceViewElement extends UmbLitElement {
	override render() {
		return html` <umb-collection alias=${UMB_USER_COLLECTION_ALIAS}></umb-collection>; `;
	}
}

export { UmbCollectionWorkspaceViewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbCollectionWorkspaceViewElement;
	}
}
