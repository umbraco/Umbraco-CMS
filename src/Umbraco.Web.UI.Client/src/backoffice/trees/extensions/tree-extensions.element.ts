import { html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbTreeBase } from '../shared/tree-base.element';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '@umbraco-cms/context-api';

import '../shared/tree-navigator.element';

@customElement('umb-tree-extensions')
export class UmbTreeExtensionsElement extends UmbContextProviderMixin(UmbContextConsumerMixin(UmbTreeBase)) {
	render() {
		return html`<umb-tree-navigator></umb-tree-navigator>`;
	}
}

export default UmbTreeExtensionsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-extensions': UmbTreeExtensionsElement;
	}
}
