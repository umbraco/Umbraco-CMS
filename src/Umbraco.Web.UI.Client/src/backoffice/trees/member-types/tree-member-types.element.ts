import { html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbTreeBase } from '../shared/tree-base.element';

import '../shared/tree-navigator.element';

@customElement('umb-tree-member-types')
export class UmbTreeMemberTypes extends UmbTreeBase {
	render() {
		return html`<umb-tree-navigator></umb-tree-navigator>`;
	}
}

export default UmbTreeMemberTypes;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-member-types': UmbTreeMemberTypes;
	}
}
