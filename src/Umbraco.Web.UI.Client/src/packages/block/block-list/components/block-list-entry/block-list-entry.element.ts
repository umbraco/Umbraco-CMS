import { UmbBlockListEntryContext } from '../../context/block-list-entry.context.js';
import { UMB_BLOCK_LIST, type UmbBlockListLayoutModel } from '../../types.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { html, css, customElement, property, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import '../ref-list-block/index.js';
import '../inline-list-block/index.js';
import { stringOrStringArrayContains } from '@umbraco-cms/backoffice/utils';
import { UmbObserveValidationStateController } from '@umbraco-cms/backoffice/validation';
import { UmbDataPathBlockElementDataQuery } from '@umbraco-cms/backoffice/block';
import type {
	ManifestBlockEditorCustomView,
	UmbBlockEditorCustomViewProperties,
} from '@umbraco-cms/backoffice/block-custom-view';

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

		new UmbObserveValidationStateController(
			this,
			`$.contentData[${UmbDataPathBlockElementDataQuery({ udi: value })}]`,
			(hasMessages) => {
				this._contentInvalid = hasMessages;
				this._blockViewProps.contentInvalid = hasMessages;
			},
			'observeMessagesForContent',
		);
	}
	private _contentUdi?: string | undefined;

	/**
	 * Sets the element to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	public readonly = false;

	#context = new UmbBlockListEntryContext(this);

	@state()
	_contentTypeAlias?: string;

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

	// 'content-invalid' attribute is used for styling purpose.
	@property({ type: Boolean, attribute: 'content-invalid', reflect: true })
	_contentInvalid?: boolean;

	// 'settings-invalid' attribute is used for styling purpose.
	@property({ type: Boolean, attribute: 'settings-invalid', reflect: true })
	_settingsInvalid?: boolean;

	@state()
	_blockViewProps: UmbBlockEditorCustomViewProperties<UmbBlockListLayoutModel> = {
		contentUdi: undefined!,
		config: { showContentEdit: false, showSettingsEdit: false },
	}; // Set to undefined cause it will be set before we render.

	#updateBlockViewProps(incoming: Partial<UmbBlockEditorCustomViewProperties<UmbBlockListLayoutModel>>) {
		this._blockViewProps = { ...this._blockViewProps, ...incoming };
		this.requestUpdate('_blockViewProps');
	}

	constructor() {
		super();

		this.observe(
			this.#context.showContentEdit,
			(showContentEdit) => {
				this._showContentEdit = showContentEdit;
				this.#updateBlockViewProps({ config: { ...this._blockViewProps.config, showContentEdit } });
			},
			null,
		);
		this.observe(
			this.#context.settingsElementTypeKey,
			(key) => {
				this._hasSettings = !!key;
				this.#updateBlockViewProps({ config: { ...this._blockViewProps.config, showSettingsEdit: !!key } });
			},
			null,
		);
		this.observe(
			this.#context.blockType,
			(blockType) => {
				this.#updateBlockViewProps({ blockType });
			},
			null,
		);
		// TODO: Implement index.
		this.observe(
			this.#context.label,
			(label) => {
				this.#updateBlockViewProps({ label });
				this._label = label;
			},
			null,
		);
		this.observe(
			this.#context.contentElementTypeIcon,
			(icon) => {
				this.#updateBlockViewProps({ icon });
				this._icon = icon;
			},
			null,
		);
		this.observe(
			this.#context.inlineEditingMode,
			(inlineEditingMode) => {
				this._inlineEditingMode = inlineEditingMode;
			},
			null,
		);
		// Data props:
		this.observe(
			this.#context.layout,
			(layout) => {
				this.#updateBlockViewProps({ layout });
			},
			null,
		);
		this.observe(
			this.#context.content,
			(content) => {
				this.#updateBlockViewProps({ content });
			},
			null,
		);
		this.observe(
			this.#context.settings,
			(settings) => {
				this.#updateBlockViewProps({ settings });

				this.removeUmbControllerByAlias('observeMessagesForSettings');
				if (settings) {
					// Observe settings validation state:
					new UmbObserveValidationStateController(
						this,
						`$.settingsData[${UmbDataPathBlockElementDataQuery(settings)}]`,
						(hasMessages) => {
							this._settingsInvalid = hasMessages;
							this._blockViewProps.settingsInvalid = hasMessages;
						},
						'observeMessagesForSettings',
					);
				}
			},
			null,
		);
		this.observe(
			this.#context.workspaceEditContentPath,
			(path) => {
				this._workspaceEditContentPath = path;
				this.#updateBlockViewProps({ config: { ...this._blockViewProps.config, editContentPath: path } });
			},
			null,
		);
		this.observe(
			this.#context.workspaceEditSettingsPath,
			(path) => {
				this._workspaceEditSettingsPath = path;
				this.#updateBlockViewProps({ config: { ...this._blockViewProps.config, editSettingsPath: path } });
			},
			null,
		);
	}

	override connectedCallback(): void {
		super.connectedCallback();
		// element styling:
		this.observe(
			this.#context.contentElementTypeKey,
			(contentElementTypeKey) => {
				if (contentElementTypeKey) {
					this.setAttribute('data-content-element-type-key', contentElementTypeKey);
				}
			},
			'contentElementTypeKey',
		);
		this.observe(
			this.#context.contentElementTypeAlias,
			(contentElementTypeAlias) => {
				if (contentElementTypeAlias) {
					this._contentTypeAlias = contentElementTypeAlias;
					this.setAttribute('data-content-element-type-alias', contentElementTypeAlias);
				}
			},
			'contentElementTypeAlias',
		);
	}

	#extensionSlotFilterMethod = (manifest: ManifestBlockEditorCustomView) => {
		if (
			manifest.forContentTypeAlias &&
			!stringOrStringArrayContains(manifest.forContentTypeAlias, this._contentTypeAlias!)
		) {
			return false;
		}
		if (manifest.forBlockEditor && !stringOrStringArrayContains(manifest.forBlockEditor, UMB_BLOCK_LIST)) {
			return false;
		}
		return true;
	};

	#renderRefBlock() {
		return html`<umb-ref-list-block .label=${this._label} .icon=${this._icon}></umb-ref-list-block>`;
	}

	#renderInlineBlock() {
		return html`<umb-inline-list-block .label=${this._label} .icon=${this._icon}></umb-inline-list-block>`;
	}

	#renderBlock() {
		return html`
			<umb-extension-slot
				type="blockEditorCustomView"
				default-element=${this._inlineEditingMode ? 'umb-inline-list-block' : 'umb-ref-list-block'}
				.props=${this._blockViewProps}
				.filter=${this.#extensionSlotFilterMethod}
				single
				>${this._inlineEditingMode ? this.#renderInlineBlock() : this.#renderRefBlock()}</umb-extension-slot
			>
			<uui-action-bar>
				${this.#renderEditContentAction()} ${this.#renderEditSettingsAction()} ${this.#renderDeleteAction()}
			</uui-action-bar>
			${!this._showContentEdit && this._contentInvalid
				? html`<uui-badge attention color="danger" label="Invalid content">!</uui-badge>`
				: nothing}
		`;
	}

	#renderEditContentAction() {
		return html` ${this._showContentEdit && this._workspaceEditContentPath
			? html`<uui-button
					label="edit"
					look="secondary"
					color=${this._contentInvalid ? 'danger' : ''}
					href=${this._workspaceEditContentPath}>
					<uui-icon name="icon-edit"></uui-icon>
					${this._contentInvalid
						? html`<uui-badge attention color="danger" label="Invalid content">!</uui-badge>`
						: nothing}
				</uui-button>`
			: nothing}`;
	}

	#renderEditSettingsAction() {
		return html`
			${this._hasSettings && this._workspaceEditSettingsPath
				? html`<uui-button
						label="Edit settings"
						look="secondary"
						color=${this._settingsInvalid ? 'danger' : ''}
						href=${this._workspaceEditSettingsPath}>
						<uui-icon name="icon-settings"></uui-icon>
						${this._settingsInvalid
							? html`<uui-badge attention color="danger" label="Invalid settings">!</uui-badge>`
							: nothing}
					</uui-button>`
				: nothing}
		`;
	}

	#renderDeleteAction() {
		if (this.readonly) return nothing;
		return html` <uui-button label="delete" look="secondary" @click=${() => this.#context.requestDelete()}>
			<uui-icon name="icon-remove"></uui-icon>
		</uui-button>`;
	}

	override render() {
		return this.#renderBlock();
	}

	static override styles = [
		css`
			:host {
				position: relative;
				display: block;
				--umb-block-list-entry-actions-opacity: 0;
			}

			:host([settings-invalid]),
			:host([content-invalid]),
			:host(:hover),
			:host(:focus-within) {
				--umb-block-list-entry-actions-opacity: 1;
			}

			uui-action-bar {
				position: absolute;
				top: var(--uui-size-2);
				right: var(--uui-size-2);
				opacity: var(--umb-block-list-entry-actions-opacity, 0);
				transition: opacity 120ms;
			}

			:host([drag-placeholder]) {
				opacity: 0.2;
				--umb-block-list-entry-actions-opacity: 0;
			}

			:host([settings-invalid])::after,
			:host([content-invalid])::after {
				content: '';
				position: absolute;
				inset: 0;
				pointer-events: none;
				border: 1px solid var(--uui-color-danger);
				border-radius: var(--uui-border-radius);
			}

			uui-badge {
				z-index: 2;
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
