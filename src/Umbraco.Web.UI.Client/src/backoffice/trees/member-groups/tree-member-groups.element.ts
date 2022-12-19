import { html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbTreeBase } from '../shared/tree-base.element';

import '../shared/tree-navigator.element';

@customElement('umb-tree-member-groups')
export class UmbTreeMemberGroups extends UmbTreeBase {
	render() {
		return html`<umb-tree-navigator store-context-alias="umbMemberGroupStore"></umb-tree-navigator>`;
	}
}

export default UmbTreeMemberGroups;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-member-groups': UmbTreeMemberGroups;
	}
}
