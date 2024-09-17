import { UMB_MEMBER_COLLECTION_ALIAS } from '../../collection/index.js';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const elementName = 'umb-member-root-workspace';
@customElement(elementName)
export class UmbMemberRootWorkspaceElement extends UmbLitElement {
	override render() {
		return html` <umb-collection alias=${UMB_MEMBER_COLLECTION_ALIAS}></umb-collection>; `;
	}
}

export { UmbMemberRootWorkspaceElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbMemberRootWorkspaceElement;
	}
}
