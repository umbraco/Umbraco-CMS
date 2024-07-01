import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { html, css, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { ManifestBlockEditorCustomView, UmbBlockEditorCustomViewProperties, UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import '../ref-list-block/index.js';
import '../inline-list-block/index.js';
import { stringOrStringArrayContains } from '@umbraco-cms/backoffice/utils';
import { UmbBlockListEntryContext } from '../../context/block-list-entry.context.js';
import { UMB_BLOCK_LIST, type UmbBlockListLayoutModel } from '../../types.js';

/**
 * @element umb-block-list-entry
 */
@customElement('umb-block-list-entry')
export class UmbBlockListEntryElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	//
	@property({ type: Number })
	public get index(): number | undefined {
		return this.#context.getIndex();
	}
	public set index(value: number | undefined) {
		this.#context.setIndex(value);
	}

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
	_contentTypeAlias?:string;

	@state()
	_showContentEdit = false;
	@state()
	_hasSettings = false;

	@state()
	_label = '';

	@state()
	_icon?: string;

	@state()
	_workspaceEditContentPath?: string;

	@state()
	_workspaceEditSettingsPath?: string;

	@state()
	_inlineEditingMode?: boolean;

	@state()
	_blockViewProps: UmbBlockEditorCustomViewProperties<UmbBlockListLayoutModel> = { contentUdi: undefined!, urls: {} }; // Set to undefined cause it will be set before we render.

	#updateBlockViewProps(incoming: Partial<UmbBlockEditorCustomViewProperties<UmbBlockListLayoutModel>>) {
		this._blockViewProps = { ...this._blockViewProps, ...incoming };
		this.requestUpdate('_blockViewProps');
	}

	constructor() {
		super();

		this.observe(this.#context.showContentEdit, (showContentEdit) => {
			this._showContentEdit = showContentEdit;
		}, null);
		this.observe(this.#context.settingsElementTypeKey, (settingsElementTypeKey) => {
			this._hasSettings = !!settingsElementTypeKey;
		}, null);
		this.observe(this.#context.label, (label) => {
			this.#updateBlockViewProps({ label });
			this._label = label;
		}, null);
		this.observe(this.#context.contentElementTypeIcon, (icon) => {
			this.#updateBlockViewProps({ icon });
			this._icon = icon;
		}, null);
		this.observe(this.#context.inlineEditingMode, (inlineEditingMode) => {
			this._inlineEditingMode = inlineEditingMode;
		}, null);
		// Data props:
		this.observe(this.#context.layout, (layout) => {
			this.#updateBlockViewProps({ layout });
		}, null);
		this.observe(this.#context.content, (content) => {
			this.#updateBlockViewProps({ content });
		}, null);
		this.observe(this.#context.settings, (settings) => {
			this.#updateBlockViewProps({ settings });
		}, null);
		this.observe(this.#context.workspaceEditContentPath, (path) => {
			this._workspaceEditContentPath = path;
			this.#updateBlockViewProps({ urls: { ...this._blockViewProps.urls, editContent: path } });
		}, null);
		this.observe(this.#context.workspaceEditSettingsPath, (path) => {
			this._workspaceEditSettingsPath = path;
			this.#updateBlockViewProps({ urls: { ...this._blockViewProps.urls, editSettings: path } });
		}, null);
	}


	override connectedCallback(): void {
		super.connectedCallback();
		// element styling:
		this.observe(this.#context.contentElementTypeKey, (contentElementTypeKey) => {
			if (contentElementTypeKey) {
				this.setAttribute('data-content-element-type-key', contentElementTypeKey);
			}
		}, 'contentElementTypeKey');
		this.observe(this.#context.contentElementTypeAlias, (contentElementTypeAlias) => {
			if (contentElementTypeAlias) {
				this._contentTypeAlias = contentElementTypeAlias;
				this.setAttribute('data-content-element-type-alias', contentElementTypeAlias);
			}
		}, 'contentElementTypeAlias');
	}

	#extensionSlotFilterMethod = (manifest:ManifestBlockEditorCustomView) => {
		if(manifest.forContentTypeAlias && !stringOrStringArrayContains(manifest.forContentTypeAlias, this._contentTypeAlias!)) {
			return false;
		}
		if(manifest.forBlockEditor && !stringOrStringArrayContains(manifest.forBlockEditor, UMB_BLOCK_LIST)) {
			return false;
		}
		return true;
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
				.filter=${this.#extensionSlotFilterMethod}
				>${this._inlineEditingMode ? this.#renderInlineBlock() : this.#renderRefBlock()}</umb-extension-slot
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
		`;
	}

	override render() {
		return this.#renderBlock();
	}

	static override styles = [
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
