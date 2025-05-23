import type { UmbBlockRteLayoutModel } from '../../types.js';
import { UMB_BLOCK_RTE } from '../../constants.js';
import { UmbBlockRteEntryContext } from '../../context/block-rte-entry.context.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { html, css, property, state, customElement, nothing } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/property-editor';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type {
	ManifestBlockEditorCustomView,
	UmbBlockEditorCustomViewProperties,
} from '@umbraco-cms/backoffice/block-custom-view';
import { stringOrStringArrayContains } from '@umbraco-cms/backoffice/utils';

import '../ref-rte-block/index.js';
import { UmbObserveValidationStateController } from '@umbraco-cms/backoffice/validation';
import { UmbDataPathBlockElementDataQuery } from '@umbraco-cms/backoffice/block';
import type { UmbExtensionElementInitializer } from '@umbraco-cms/backoffice/extension-api';

/**
 * @class UmbBlockRteEntryElement
 */
@customElement('umb-rte-block')
export class UmbBlockRteEntryElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property({ type: String, attribute: 'data-content-key', reflect: true })
	public get contentKey(): string | undefined {
		return this._contentKey;
	}
	public set contentKey(value: string | undefined) {
		if (!value) return;
		this._contentKey = value;
		this.#context.setContentKey(value);

		new UmbObserveValidationStateController(
			this,
			`$.contentData[${UmbDataPathBlockElementDataQuery({ key: value })}]`,
			(hasMessages) => {
				this._contentInvalid = hasMessages;
				this._blockViewProps.contentInvalid = hasMessages;
			},
			'observeMessagesForContent',
		);
	}
	private _contentKey?: string | undefined;

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
	_exposed?: boolean;

	@state()
	_workspaceEditContentPath?: string;

	@state()
	_workspaceEditSettingsPath?: string;

	@state()
	_contentTypeAlias?: string;

	@state()
	_contentTypeName?: string;

	@state()
	_blockViewProps: UmbBlockEditorCustomViewProperties<UmbBlockRteLayoutModel> = {
		contentKey: undefined!,
		config: { showContentEdit: false, showSettingsEdit: false },
	}; // Set to undefined cause it will be set before we render.

	// 'content-invalid' attribute is used for styling purpose.
	@property({ type: Boolean, attribute: 'content-invalid', reflect: true })
	_contentInvalid?: boolean;

	// 'settings-invalid' attribute is used for styling purpose.
	@property({ type: Boolean, attribute: 'settings-invalid', reflect: true })
	_settingsInvalid?: boolean;

	#updateBlockViewProps(incoming: Partial<UmbBlockEditorCustomViewProperties<UmbBlockRteLayoutModel>>) {
		this._blockViewProps = { ...this._blockViewProps, ...incoming };
		this.requestUpdate('_blockViewProps');
	}

	constructor() {
		super();

		// We do not have index for RTE Blocks at the moment.
		this.#context.setIndex(0);

		this.observe(
			this.#context.showContentEdit,
			(showContentEdit) => {
				this._showContentEdit = showContentEdit;
				this.#updateBlockViewProps({ config: { ...this._blockViewProps.config!, showContentEdit } });
			},
			null,
		);
		this.observe(
			this.#context.settingsElementTypeKey,
			(key) => {
				this._hasSettings = !!key;
				this.#updateBlockViewProps({ config: { ...this._blockViewProps.config!, showSettingsEdit: !!key } });
			},
			null,
		);
		this.observe(
			this.#context.contentElementTypeAlias,
			(alias) => {
				this._contentTypeAlias = alias;
			},
			null,
		);
		this.observe(
			this.#context.contentElementTypeName,
			(contentElementTypeName) => {
				this._contentTypeName = contentElementTypeName;
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
			this.#context.hasExpose,
			(exposed) => {
				this.#updateBlockViewProps({ unpublished: !exposed });
				this._exposed = exposed;
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

		this.#observeData();

		this.observe(
			this.#context.settingsKey,
			(settingsKey) => {
				this.removeUmbControllerByAlias('observeMessagesForSettings');
				if (settingsKey) {
					// Observe settings validation state:
					new UmbObserveValidationStateController(
						this,
						`$.settingsData[${UmbDataPathBlockElementDataQuery({ key: settingsKey })}]`,
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
				this.#updateBlockViewProps({ config: { ...this._blockViewProps.config!, editContentPath: path } });
			},
			null,
		);
		this.observe(
			this.#context.workspaceEditSettingsPath,
			(path) => {
				this._workspaceEditSettingsPath = path;
				this.#updateBlockViewProps({ config: { ...this._blockViewProps.config!, editSettingsPath: path } });
			},
			null,
		);
	}

	async #observeData() {
		this.observe(
			await this.#context.contentValues(),
			(content) => {
				this.#updateBlockViewProps({ content });
			},
			null,
		);
		this.observe(
			await this.#context.settingsValues(),
			(settings) => {
				this.#updateBlockViewProps({ settings });
			},
			null,
		);
	}

	override connectedCallback() {
		super.connectedCallback();
		this.setAttribute('contenteditable', 'false');
	}

	readonly #filterBlockCustomViews = (manifest: ManifestBlockEditorCustomView) => {
		const elementTypeAlias = this._contentTypeAlias ?? '';
		const isForBlockEditor =
			!manifest.forBlockEditor || stringOrStringArrayContains(manifest.forBlockEditor, UMB_BLOCK_RTE);
		const isForContentTypeAlias =
			!manifest.forContentTypeAlias || stringOrStringArrayContains(manifest.forContentTypeAlias, elementTypeAlias);
		return isForBlockEditor && isForContentTypeAlias;
	};

	#expose = () => {
		this.#context.expose();
	};

	#extensionSlotRenderMethod = (ext: UmbExtensionElementInitializer<ManifestBlockEditorCustomView>) => {
		ext.component?.setAttribute('part', 'component');
		if (this._exposed) {
			return ext.component;
		} else {
			return html`<div>
				${ext.component}
				<umb-block-overlay-expose-button
					.contentTypeName=${this._contentTypeName}
					@click=${this.#expose}></umb-block-overlay-expose-button>
			</div>`;
		}
	};

	#renderBlock() {
		return this.contentKey && this._contentTypeAlias
			? html`
					<div class="uui-text uui-font">
						<umb-extension-slot
							type="blockEditorCustomView"
							default-element="umb-ref-rte-block"
							.renderMethod=${this.#extensionSlotRenderMethod}
							.props=${this._blockViewProps}
							.filter=${this.#filterBlockCustomViews}
							single>
							${this.#renderRefBlock()}
						</umb-extension-slot>
						<uui-action-bar> ${this.#renderEditAction()} ${this.#renderEditSettingsAction()} </uui-action-bar>
						${!this._showContentEdit && this._contentInvalid
							? html`<uui-badge attention color="invalid" label="Invalid content">!</uui-badge>`
							: nothing}
					</div>
				`
			: nothing;
	}

	#renderRefBlock() {
		return html`<umb-ref-rte-block
			.label=${this._label}
			.icon=${this._icon}
			.unpublished=${!this._exposed}
			.content=${this._blockViewProps.content}
			.settings=${this._blockViewProps.settings}></umb-ref-rte-block>`;
	}

	#renderEditAction() {
		return this._showContentEdit && this._workspaceEditContentPath
			? html`<uui-button
					label="edit"
					look="secondary"
					color=${this._contentInvalid ? 'invalid' : ''}
					href=${this._workspaceEditContentPath}>
					<uui-icon name=${this._exposed === false ? 'icon-add' : 'icon-edit'}></uui-icon>
					${this._contentInvalid
						? html`<uui-badge attention color="invalid" label="Invalid content">!</uui-badge>`
						: nothing}
				</uui-button>`
			: this._showContentEdit === false && this._exposed === false
				? html`<uui-button
						@click=${this.#expose}
						label=${this.localize.term('blockEditor_createThisFor', this._contentTypeName)}
						look="secondary">
						<uui-icon name="icon-add"></uui-icon>
					</uui-button>`
				: nothing;
	}

	#renderEditSettingsAction() {
		return html`
			${this._hasSettings && this._workspaceEditSettingsPath
				? html`<uui-button
						label="Edit settings"
						look="secondary"
						color=${this._settingsInvalid ? 'invalid' : ''}
						href=${this._workspaceEditSettingsPath}>
						<uui-icon name="icon-settings"></uui-icon>
						${this._settingsInvalid
							? html`<uui-badge attention color="invalid" label="Invalid settings">!</uui-badge>`
							: nothing}
					</uui-button>`
				: nothing}
		`;
	}

	override render() {
		return this.#renderBlock();
	}

	static override readonly styles = [
		UmbTextStyles,
		css`
			:host {
				position: relative;
				display: block;
				user-select: none;
				user-drag: auto;
				white-space: nowrap;
			}
			:host(.ProseMirror-selectednode) {
				umb-ref-rte-block {
					cursor: not-allowed;
					outline: 3px solid var(--uui-color-focus);
				}
			}

			umb-extension-slot::part(component) {
				position: relative;
				z-index: 0;
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
