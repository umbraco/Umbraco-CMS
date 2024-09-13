import { UMB_MEMBER_GROUP_COLLECTION_ALIAS } from '../../collection/index.js';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const elementName = 'umb-member-group-root-workspace';
@customElement(elementName)
export class UmbMemberGroupRootWorkspaceElement extends UmbLitElement {
	override render() {
		return html` <umb-body-layout main-no-padding headline=${this.localize.term('member_membergroups')}>
			<umb-collection alias=${UMB_MEMBER_GROUP_COLLECTION_ALIAS}></umb-collection>;
		</umb-body-layout>`;
	}
}

export { UmbMemberGroupRootWorkspaceElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbMemberGroupRootWorkspaceElement;
	}
}
