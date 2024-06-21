import { UmbBlockRteEntryContext } from '../../context/block-rte-entry.context.js';
import type { UmbBlockRteLayoutModel } from '../../types.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { html, css, property, state, customElement } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import '../ref-rte-block/index.js';
import type { UmbBlockViewPropsType } from '@umbraco-cms/backoffice/block';

/**
 * @element umb-rte-block
 * @element umb-rte-block-inline
 */
@customElement('umb-rte-block')
export class UmbBlockRteEntryElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	//
	@property({ type: String, attribute: 'data-content-udi', reflect: true })
	public get contentUdi(): string | undefined {
		return this._contentUdi;
	}
	public set contentUdi(value: string | undefined) {
		if (!value) return;
		this._contentUdi = value;
		this.#context.setContentUdi(value);
	}
	private _contentUdi?: string | undefined;

	#context = new UmbBlockRteEntryContext(this);

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

	// TODO: use this type on the Element Interface for the Manifest.
	@state()
	_blockViewProps: UmbBlockViewPropsType<UmbBlockRteLayoutModel> = { contentUdi: undefined!, urls: {} }; // Set to undefined cause it will be set before we render.

	constructor() {
		super();

		// We do not have index for RTE Blocks at the moment.
		this.#context.setIndex(0);

		this.observe(this.#context.showContentEdit, (showContentEdit) => {
			this._showContentEdit = showContentEdit;
		});
		this.observe(this.#context.settingsElementTypeKey, (settingsElementTypeKey) => {
			this._hasSettings = !!settingsElementTypeKey;
		});
		this.observe(this.#context.label, (label) => {
			this._label = label;
			this._blockViewProps.label = label;
			this.requestUpdate('_blockViewProps');
		});
		this.observe(this.#context.contentElementTypeIcon, (icon) => {
			this._icon = icon;
			this._blockViewProps.icon = icon;
			this.requestUpdate('_blockViewProps');
		});
		// Data props:
		this.observe(this.#context.layout, (layout) => {
			this._blockViewProps.layout = layout;
		});
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

	override connectedCallback() {
		super.connectedCallback();

		// eslint-disable-next-line wc/no-self-class
		this.classList.add('uui-font');
		// eslint-disable-next-line wc/no-self-class
		this.classList.add('uui-text');

		this.setAttribute('contenteditable', 'false');
	}

	#renderRefBlock() {
		return html`<umb-ref-rte-block .label=${this._label} .icon=${this._icon}></umb-ref-rte-block>`;
	}

	#renderBlock() {
		return html`
			<umb-extension-slot
				type="blockEditorCustomView"
				default-element=${'umb-ref-rte-block'}
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
				user-select: none;
				user-drag: auto;
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

export default UmbBlockRteEntryElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-rte-block': UmbBlockRteEntryElement;
	}
}
