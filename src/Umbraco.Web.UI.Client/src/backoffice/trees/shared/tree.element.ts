import { html, LitElement } from 'lit';
import { when } from 'lit-html/directives/when.js';
import { customElement, property, state } from 'lit/decorators.js';
import { map } from 'rxjs';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '../../../core/context';
import { UmbExtensionRegistry } from '../../../core/extension';
import type { ManifestTree } from '../../../core/models';
import { UmbObserverMixin } from '../../../core/observer';
import { UmbTreeContextBase } from '../tree.context';

@customElement('umb-tree')
export class UmbTreeElement extends UmbContextProviderMixin(UmbContextConsumerMixin(UmbObserverMixin(LitElement))) {
	private _alias = '';
	@property({ type: String, reflect: true })
	get alias() {
		return this._alias;
	}
	set alias(newVal) {
		const oldVal = this._alias;
		this._alias = newVal;
		this.requestUpdate('alias', oldVal);
		this._observeTree();
	}

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

	@state()
	private _tree?: ManifestTree;

	private _treeContext?: UmbTreeContextBase;
	private _extensionRegistry?: UmbExtensionRegistry;

	constructor() {
		super();
		this.consumeContext('umbExtensionRegistry', (extensionRegistry) => {
			this._extensionRegistry = extensionRegistry;
			this._observeTree();
		});
	}

	private _observeTree() {
		if (!this._extensionRegistry || !this.alias) return;

		this.observe<ManifestTree>(
			this._extensionRegistry
				.extensionsOfType('tree')
				.pipe(map((trees) => trees.find((tree) => tree.alias === this.alias))),
			(tree) => {
				this._tree = tree;
				this._provideTreeContext();
			}
		);
	}

	private _provideTreeContext() {
		if (!this._tree) return;
		this._treeContext = new UmbTreeContextBase(this._tree);
		this._treeContext.setSelectable(this.selectable);
		this._treeContext.setSelection(this.selection);
		this.provideContext('umbTreeContext', this._treeContext);

		if (this.selectable) {
			this._observeSelection();
		}
	}

	private _observeSelection() {
		if (!this._treeContext) return;

		this.observe(this._treeContext.selection, (selection) => {
			this._selection = selection;
			this.dispatchEvent(new CustomEvent('change'));
		});
	}

	render() {
		return html`${when(this._tree, () => html`<umb-tree-extension .tree=${this._tree}></umb-tree-extension>`)}`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree': UmbTreeElement;
	}
}
