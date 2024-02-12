import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbBlockGridEntryContext } from '../../context/block-grid-entry.context.js';
import { html, css, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import '../ref-grid-block/index.js';
import type { UmbBlockGridLayoutModel, UmbBlockViewPropsType } from '@umbraco-cms/backoffice/block';

/**
 * @element umb-block-grid-entry
 */
@customElement('umb-block-grid-entry')
export class UmbBlockGridEntryElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	//
	@property({ attribute: false })
	public get contentUdi(): string | undefined {
		return this._contentUdi;
	}
	public set contentUdi(value: string | undefined) {
		if (!value) return;
		this._contentUdi = value;
		this._blockViewProps.contentUdi = value;
		this.#context.setContentUdi(value);
	}
	private _contentUdi?: string | undefined;
	//

	#context = new UmbBlockGridEntryContext(this);

	@state()
	_showContentEdit = false;
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
	_blockViewProps: UmbBlockViewPropsType<UmbBlockGridLayoutModel> = { contentUdi: undefined!, urls: {} }; // Set to undefined cause it will be set before we render.

	constructor() {
		super();

		this.observe(this.#context.showContentEdit, (showContentEdit) => {
			this._showContentEdit = showContentEdit;
		});
		this.observe(this.#context.blockTypeSettingsElementTypeKey, (blockTypeSettingsElementTypeKey) => {
			this._hasSettings = !!blockTypeSettingsElementTypeKey;
		});
		this.observe(this.#context.label, (label) => {
			this._blockViewProps.label = label;
			this._label = label;
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
		return html`<umb-ref-grid-block .contentUdi=${this.contentUdi} .label=${this._label}></umb-ref-grid-block>`;
	}

	#renderBlock() {
		return this.contentUdi
			? html`
					<umb-extension-slot
						type="blockEditorCustomView"
						default-element=${'umb-ref-grid-block'}
						.props=${this._blockViewProps}
						>${this.#renderRefBlock()}</umb-extension-slot
					>
					<uui-action-bar>
						${this._showContentEdit && this._workspaceEditContentPath
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
			  `
			: '';
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

export default UmbBlockGridEntryElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-grid-entry': UmbBlockGridEntryElement;
	}
}
