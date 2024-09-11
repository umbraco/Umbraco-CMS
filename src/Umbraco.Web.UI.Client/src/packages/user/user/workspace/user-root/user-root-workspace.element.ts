import { UMB_USER_COLLECTION_ALIAS } from '../../collection/index.js';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const elementName = 'umb-user-root-workspace';
@customElement(elementName)
export class UmbUserRootWorkspaceElement extends UmbLitElement {
	override render() {
		return html` <umb-body-layout main-no-padding headline="Users">
			<umb-collection alias=${UMB_USER_COLLECTION_ALIAS}></umb-collection>;
		</umb-body-layout>`;
	}
}

export { UmbUserRootWorkspaceElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbUserRootWorkspaceElement;
	}
}
