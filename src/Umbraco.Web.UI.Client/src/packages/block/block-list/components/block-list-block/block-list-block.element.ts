import { html, css, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { type UmbBlockLayoutBaseModel } from '@umbraco-cms/backoffice/block';
import '../ref-list-block/index.js';
import { UmbBlockContext } from '@umbraco-cms/backoffice/block';

/**
 * @element umb-property-editor-ui-block-list-block
 */
@customElement('umb-property-editor-ui-block-list-block')
export class UmbPropertyEditorUIBlockListBlockElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	//
	@property({ attribute: false })
	public get layout(): UmbBlockLayoutBaseModel | undefined {
		return this._layout;
	}
	public set layout(value: UmbBlockLayoutBaseModel | undefined) {
		this._layout = value;
		this.#context.setLayout(value);
	}
	private _layout?: UmbBlockLayoutBaseModel | undefined;

	#context = new UmbBlockContext(this);

	@state()
	_label = '';

	constructor() {
		super();

		this.observe(this.#context.label, (label) => {
			this._label = label;
		});
	}

	#renderRefBlock() {
		return html`<umb-ref-list-block .name=${this._label}> </umb-ref-list-block>`;
	}

	/*#renderInlineBlock() {
		return html`<umb-inline-list-block name="block" }}></umb-inline-list-block>`;
	}*/

	#renderBlock() {
		return html`
			${this.#renderRefBlock()}
			<uui-action-bar>
				<uui-button label="remove" compact>
					<uui-icon name="icon-remove"></uui-icon>
				</uui-button>
			</uui-action-bar>
		`;
	}

	render() {
		return this.layout ? this.#renderBlock() : '';
	}

	static styles = [
		css`
			:host {
				position: relative;
				display: block;
			}
			uui-action-bar {
				position: absolute;
				top: 0;
				right: 0;
			}
		`,
	];
}

export default UmbPropertyEditorUIBlockListBlockElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-list-block': UmbPropertyEditorUIBlockListBlockElement;
	}
}
