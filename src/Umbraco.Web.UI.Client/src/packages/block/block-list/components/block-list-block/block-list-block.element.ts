import { UmbBlockListContext } from '../../context/block-list.context.js';
import { html, css, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbBlockLayoutBaseModel } from '@umbraco-cms/backoffice/block';
import '../ref-list-block/index.js';
import '../inline-list-block/index.js';
import { UMB_CONFIRM_MODAL, UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

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

	#context = new UmbBlockListContext(this);

	@state()
	_contentUdi?: string;

	@state()
	_label = '';

	@state()
	_workspaceEditPath?: string;

	@state()
	_inlineEditingMode?: boolean;

	constructor() {
		super();

		this.observe(this.#context.workspaceEditPath, (workspaceEditPath) => {
			this._workspaceEditPath = workspaceEditPath;
		});
		this.observe(this.#context.contentUdi, (contentUdi) => {
			this._contentUdi = contentUdi;
		});
		this.observe(this.#context.label, (label) => {
			this._label = label;
		});
		this.observe(this.#context.inlineEditingMode, (inlineEditingMode) => {
			this._inlineEditingMode = inlineEditingMode;
		});
	}

	#requestDelete() {
		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, async (modalManager) => {
			const modalContext = modalManager.open(UMB_CONFIRM_MODAL, {
				data: {
					headline: `Delete ${this._label}`,
					content: 'Are you sure you want to delete this [INSERT BLOCK TYPE NAME]?',
					confirmLabel: 'Delete',
					color: 'danger',
				},
			});
			await modalContext.onSubmit();
			this.#context.delete();
		});
	}

	#renderRefBlock() {
		return html`<umb-ref-list-block .name=${this._label}></umb-ref-list-block>`;
	}

	#renderInlineBlock() {
		return html`<umb-inline-list-block .name=${this._label}></umb-inline-list-block>`;
	}

	#renderBlock() {
		return html`
			${this._inlineEditingMode ? this.#renderInlineBlock() : this.#renderRefBlock()}
			<uui-action-bar>
				${this._workspaceEditPath
					? html`<uui-button label="edit" compact href=${this._workspaceEditPath}>
							<uui-icon name="icon-edit"></uui-icon>
					  </uui-button>`
					: ''}
				<uui-button label="delete" compact @click=${this.#requestDelete}>
					<uui-icon name="icon-remove"></uui-icon>
				</uui-button>
			</uui-action-bar>
		`;
	}

	render() {
		return this.layout && this._contentUdi ? this.#renderBlock() : '';
	}

	static styles = [
		css`
			:host {
				position: relative;
				display: block;
			}
			uui-action-bar {
				position: absolute;
				top: var(--uui-size-2);
				right: var(--uui-size-2);
			}

			:host([drag-placeholder]) {
				opacity: 0.2;
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
