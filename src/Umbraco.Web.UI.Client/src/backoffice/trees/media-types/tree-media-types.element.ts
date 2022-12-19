import { html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbTreeBase } from '../shared/tree-base.element';

import '../shared/tree-navigator.element';

@customElement('umb-tree-media-types')
export class UmbTreeMediaTypes extends UmbTreeBase {
	render() {
		return html`<umb-tree-navigator store-context-alias="umbMediaTypeStore"></umb-tree-navigator>`;
	}
}

export default UmbTreeMediaTypes;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-media-types': UmbTreeMediaTypes;
	}
}
