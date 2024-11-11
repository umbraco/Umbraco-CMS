import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbDefaultTreeElement } from '@umbraco-cms/backoffice/tree';

const elementName = 'umb-media-tree';
@customElement(elementName)
export class UmbMediaTreeElement extends UmbDefaultTreeElement {}

export { UmbMediaTreeElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbMediaTreeElement;
	}
}
