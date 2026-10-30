import type { Editor } from '../../externals.js';
import { css, customElement, html, join, map, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-tiptap-statusbar-element-path')
export class UmbTiptapStatusbarElementPathElement extends UmbLitElement {
	@state()
	private _path?: Array<string>;

	public editor?: Editor;

	override connectedCallback() {
		super.connectedCallback();

		if (this.editor) {
			this.editor.on('selectionUpdate', this.#onEditorSelectionUpdate);
			this.#onEditorSelectionUpdate();
		}
	}

	override disconnectedCallback() {
		super.disconnectedCallback();

		if (this.editor) {
			this.editor.off('selectionUpdate', this.#onEditorSelectionUpdate);
		}
	}

	readonly #onEditorSelectionUpdate = () => {
		let dom = this.editor?.view.domAtPos(this.editor!.state.selection.from).node;

		this._path = [];

		while (dom) {
			if (!this.editor?.view.dom.contains(dom)) break;

			if (dom.nodeType === dom.ELEMENT_NODE && dom instanceof HTMLElement) {
				let tagName = dom.nodeName.toLocaleLowerCase();

				if (dom.id) {
					tagName += `#${dom.id}`;
				}

				if (dom.classList.length) {
					tagName += `${['', ...dom.classList].join('.')}`;
				}

				this._path.push(tagName);
			}

			if (!dom.parentElement) break;

			dom = dom.parentElement;
		}

		this._path.reverse().shift();
	};

	override render() {
		if (!this._path) return nothing;
		return join(
			map(this._path, (item) => html`<code>${item}</code>`),
			html`<span>&gt;</span>`,
		);
	}

	static override readonly styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				gap: 0.5rem;

				font-size: var(--uui-type-small-size);
				color: var(--uui-color-text-alt);
			}
		`,
	];
}

export { UmbTiptapStatusbarElementPathElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-tiptap-statusbar-element-path': UmbTiptapStatusbarElementPathElement;
	}
}
