import { html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbTreeBase } from '../shared/tree-base.element';

import '../shared/tree-navigator.element';

@customElement('umb-tree-document-types')
export class UmbTreeDocumentTypes extends UmbTreeBase {
	render() {
		return html`<umb-tree-navigator store-context-alias="umbDocumentTypeStore"></umb-tree-navigator>`;
	}
}

export default UmbTreeDocumentTypes;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-document-types': UmbTreeDocumentTypes;
	}
}
