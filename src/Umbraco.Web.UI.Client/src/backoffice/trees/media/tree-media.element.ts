import { html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbTreeBase } from '../shared/tree-base.element';

import '../shared/tree-navigator.element';

@customElement('umb-tree-media')
export class UmbTreeMediaElement extends UmbTreeBase {
	render() {
		return html`<umb-tree-navigator></umb-tree-navigator>`;
	}
}

export default UmbTreeMediaElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-media': UmbTreeMediaElement;
	}
}
