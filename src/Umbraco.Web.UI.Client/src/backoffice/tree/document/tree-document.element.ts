import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';
import '../shared/tree-navigator.element';
import { UmbTreeDocumentContext } from './tree-document.context';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '../../../core/context';
import { UmbEntityStore } from '../../../core/stores/entity.store';

@customElement('umb-tree-document')
export class UmbTreeDocumentElement extends UmbContextProviderMixin(UmbContextConsumerMixin(LitElement)) {
	static styles = [UUITextStyles, css``];

	private _entityStore?: UmbEntityStore;
	private _treeContext?: UmbTreeDocumentContext;

	constructor() {
		super();

		this.consumeContext('umbEntityStore', (entityStore: UmbEntityStore) => {
			this._entityStore = entityStore;
			if (!this._entityStore) return;

			this._treeContext = new UmbTreeDocumentContext(this._entityStore);
			this.provideContext('umbTreeContext', this._treeContext);
		});
	}

	render() {
		return html`<umb-tree-navigator></umb-tree-navigator>`;
	}
}

export default UmbTreeDocumentElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-document': UmbTreeDocumentElement;
	}
}
