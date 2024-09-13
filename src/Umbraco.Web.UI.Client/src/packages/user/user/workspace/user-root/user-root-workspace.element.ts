import { UMB_USER_COLLECTION_ALIAS } from '../../collection/index.js';
import { UMB_USER_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UmbEntityContext } from '@umbraco-cms/backoffice/entity';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const elementName = 'umb-user-root-workspace';
@customElement(elementName)
export class UmbUserRootWorkspaceElement extends UmbLitElement {
	constructor() {
		super();
		// TODO: this.should happen automatically
		const entityContext = new UmbEntityContext(this);
		entityContext.setEntityType(UMB_USER_ROOT_ENTITY_TYPE);
		entityContext.setUnique(null);
	}

	override render() {
		return html` <umb-body-layout main-no-padding headline=${this.localize.term('treeHeaders_users')}>
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
