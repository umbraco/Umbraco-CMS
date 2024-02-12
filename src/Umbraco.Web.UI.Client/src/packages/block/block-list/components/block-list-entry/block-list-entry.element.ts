import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbBlockListEntryContext } from '../../context/block-list-entry.context.js';
import { html, css, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import '../ref-list-block/index.js';
import '../inline-list-block/index.js';
import type { UmbBlockListLayoutModel, UmbBlockViewPropsType } from '@umbraco-cms/backoffice/block';

/**
 * @element umb-block-list-entry
 */
@customElement('umb-block-list-entry')
export class UmbBlockListEntryElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	//
	@property({ attribute: false })
	public get contentUdi(): string | undefined {
		return this._contentUdi;
	}
	public set contentUdi(value: string | undefined) {
		if (!value) return;
		this._contentUdi = value;
		this.#context.setContentUdi(value);
	}
	private _contentUdi?: string | undefined;

	#context = new UmbBlockListEntryContext(this);

	@state()
	_hasSettings = false;

	@state()
	_label = '';

	@state()
	_workspaceEditContentPath?: string;

	@state()
	_workspaceEditSettingsPath?: string;

	@state()
	_inlineEditingMode?: boolean;

	// TODO: use this type on the Element Interface for the Manifest.
	@state()
	_blockViewProps: UmbBlockViewPropsType<UmbBlockListLayoutModel> = { contentUdi: undefined!, urls: {} }; // Set to undefined cause it will be set before we render.

	constructor() {
		super();

		this.observe(this.#context.workspaceEditContentPath, (workspaceEditPath) => {
			this._workspaceEditPath = workspaceEditPath;
		});
		this.observe(this.#context.blockTypeSettingsElementTypeKey, (blockTypeSettingsElementTypeKey) => {
			this._hasSettings = !!blockTypeSettingsElementTypeKey;
		});
		this.observe(this.#context.label, (label) => {
			const oldValue = this._label;
			this._blockViewProps.label = label;
			this._label = label;
			this.requestUpdate('label', oldValue);
		});
		this.observe(this.#context.inlineEditingMode, (inlineEditingMode) => {
			this._inlineEditingMode = inlineEditingMode;
		});
		// Data props:
		this.observe(this.#context.content, (content) => {
			this._blockViewProps.content = content;
		});
		this.observe(this.#context.settings, (settings) => {
			this._blockViewProps.settings = settings;
		});
		this.observe(this.#context.workspaceEditContentPath, (path) => {
			this._workspaceEditContentPath = path;
			this._blockViewProps.urls.editContent = path;
			this.requestUpdate('_blockViewProps');
		});
		this.observe(this.#context.workspaceEditSettingsPath, (path) => {
			this._workspaceEditSettingsPath = path;
			this._blockViewProps.urls.editSettings = path;
			this.requestUpdate('_blockViewProps');
		});
	}

	#renderRefBlock() {
		return html`<umb-ref-list-block .label=${this._label}></umb-ref-list-block>`;
	}

	#renderInlineBlock() {
		return html`<umb-inline-list-block .label=${this._label}></umb-inline-list-block>`;
	}

	#renderBlock() {
		return html`
			<umb-extension-slot
				type="blockEditorCustomView"
				default-element=${this._inlineEditingMode ? 'umb-inline-list-block' : 'umb-ref-list-block'}
				.props=${this._blockViewProps}
				>${this._inlineEditingMode ? this.#renderInlineBlock() : this.#renderRefBlock()}</umb-extension-slot
			>
			<uui-action-bar>
				${this._workspaceEditContentPath
					? html`<uui-button label="edit" compact href=${this._workspaceEditContentPath}>
							<uui-icon name="icon-edit"></uui-icon>
					  </uui-button>`
					: ''}
				${this._hasSettings && this._workspaceEditSettingsPath
					? html`<uui-button label="Edit settings" compact href=${this._workspaceEditSettingsPath}>
							<uui-icon name="icon-settings"></uui-icon>
					  </uui-button>`
					: ''}
				<uui-button label="delete" compact @click=${() => this.#context.requestDelete()}>
					<uui-icon name="icon-remove"></uui-icon>
				</uui-button>
			</uui-action-bar>
		`;
	}

	render() {
		return this.#renderBlock();
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

export default UmbBlockListEntryElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-list-entry': UmbBlockListEntryElement;
	}
}
