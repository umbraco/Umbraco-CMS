import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import '../shared/tree-navigator.element';
import { UmbTreeDocumentContext } from './tree-documents.context';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '../../../core/context';
import { UmbEntityStore } from '../../../core/stores/entity.store';
import { Subscription } from 'rxjs';
import type { ManifestTree } from '../../../core/models';

@customElement('umb-tree-document')
export class UmbTreeDocumentElement extends UmbContextProviderMixin(UmbContextConsumerMixin(LitElement)) {
	static styles = [UUITextStyles, css``];

	@property({ type: Object, attribute: false })
	tree?: ManifestTree;

	private _selectable = false;
	@property({ type: Boolean, reflect: true })
	get selectable() {
		return this._selectable;
	}
	set selectable(newVal) {
		const oldVal = this._selectable;
		this._selectable = newVal;
		this.requestUpdate('selectable', oldVal);
		this._treeContext?.setSelectable(newVal);
	}

	private _selection: Array<string> = [];
	@property({ type: Array })
	get selection() {
		return this._selection;
	}
	set selection(newVal: Array<string>) {
		const oldVal = this._selection;
		this._selection = newVal;
		this.requestUpdate('selection', oldVal);
		this._treeContext?.setSelection(newVal);
	}

	private _entityStore?: UmbEntityStore;
	private _treeContext?: UmbTreeDocumentContext;
	private _selectionSubscription?: Subscription;

	constructor() {
		super();

		this.consumeContext('umbEntityStore', (entityStore: UmbEntityStore) => {
			this._entityStore = entityStore;
			if (!this._entityStore || !this.tree) return;

			this._treeContext = new UmbTreeDocumentContext(this.tree, this._entityStore);
			this._treeContext.setSelectable(this.selectable);
			this._treeContext.setSelection(this.selection);
			this._observeSelection();
			this.provideContext('umbTreeContext', this._treeContext);
		});
	}

	private _observeSelection() {
		this._selectionSubscription?.unsubscribe();
		this._selectionSubscription = this._treeContext?.selection.subscribe((selection) => {
			this._selection = selection;
			this.dispatchEvent(new CustomEvent('change', { bubbles: true, composed: true }));
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
