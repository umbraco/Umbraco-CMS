import { UMB_RELATION_TYPE_COLLECTION_ALIAS } from '../../collection/index.js';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-relation-type-root-workspace')
export class UmbRelationTypeRootWorkspaceElement extends UmbLitElement {
	render() {
		return html`
			<umb-body-layout main-no-padding headline=${this.localize.term('relationType_relations')}>
				<umb-collection alias=${UMB_RELATION_TYPE_COLLECTION_ALIAS}></umb-collection>;
			</umb-body-layout>
		`;
	}
}

export { UmbRelationTypeRootWorkspaceElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-relation-type-root-workspace': UmbRelationTypeRootWorkspaceElement;
	}
}
