import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbCollectionDefaultElement } from '@umbraco-cms/backoffice/collection';

@customElement('umb-user-group-collection')
export class UmbUserGroupCollectionElement extends UmbCollectionDefaultElement {}

export { UmbUserGroupCollectionElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-collection': UmbUserGroupCollectionElement;
	}
}
