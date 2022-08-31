import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';
import '../shared/tree-navigator.element';
import { UmbTreeMediaContext } from './tree-media.context';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '../../../core/context';
import { UmbEntityStore } from '../../../core/stores/entity.store';

@customElement('umb-tree-media')
export class UmbTreeMediaElement extends UmbContextProviderMixin(UmbContextConsumerMixin(LitElement)) {
	static styles = [UUITextStyles, css``];

	private _treeContext?: UmbTreeMediaContext;

	private _entityStore?: UmbEntityStore;

	constructor() {
		super();

		this.consumeContext('umbEntityStore', (entityStore: UmbEntityStore) => {
			this._entityStore = entityStore;
			if (!this._entityStore) return;

			this._treeContext = new UmbTreeMediaContext(this._entityStore);
			this.provideContext('umbTreeContext', this._treeContext);
		});
	}

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
