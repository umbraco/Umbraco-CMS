import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbDefaultTreeElement } from '@umbraco-cms/backoffice/tree';

@customElement('umb-media-tree')
export class UmbMediaTreeElement extends UmbDefaultTreeElement {}

export { UmbMediaTreeElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-tree': UmbMediaTreeElement;
	}
}
