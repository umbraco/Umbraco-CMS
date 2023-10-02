import { UUIRefNodeElement } from '@umbraco-cms/backoffice/external/uui';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';

/**
 *  @element umb-user-group-ref
 *  @description - Component for displaying a reference to a User Group
 *  @extends UUIRefNodeElement
 */
@customElement('umb-user-group-ref')
export class UmbUserGroupRefElement extends UmbElementMixin(UUIRefNodeElement) {
	static styles = [...UUIRefNodeElement.styles];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-ref': UmbUserGroupRefElement;
	}
}
