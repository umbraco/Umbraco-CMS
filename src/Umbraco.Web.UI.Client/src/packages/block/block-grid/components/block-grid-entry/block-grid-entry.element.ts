import { UmbBlockGridEntryContext } from '../../context/block-grid-entry.context.js';
import type { UmbBlockGridLayoutModel } from '../../types.js';
import { UMB_BLOCK_GRID } from '../../constants.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { html, css, customElement, property, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/property-editor';
import { stringOrStringArrayContains } from '@umbraco-cms/backoffice/utils';

import '../block-grid-block-inline/index.js';
import '../block-grid-block-unsupported/index.js';
import '../block-grid-block/index.js';
import '../block-scale-handler/index.js';
import { UmbObserveValidationStateController } from '@umbraco-cms/backoffice/validation';
import { UmbDataPathBlockElementDataQuery } from '@umbraco-cms/backoffice/block';
import type {
	ManifestBlockEditorCustomView,
	UmbBlockEditorCustomViewProperties,
} from '@umbraco-cms/backoffice/block-custom-view';
import { UUIBlinkAnimationValue, UUIBlinkKeyframes } from '@umbraco-cms/backoffice/external/uui';
import type { UmbExtensionElementInitializer } from '@umbraco-cms/backoffice/extension-api';

/**
 * @element umb-block-grid-entry
 */
@customElement('umb-block-grid-entry')
export class UmbBlockGridEntryElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	//
	@property({ type: Number, reflect: true })
	public get index(): number | undefined {
		return this.#context.getIndex();
	}
	public set index(value: number | undefined) {
		this.#context.setIndex(value);
	}

	@property({ attribute: false })
	public get contentKey(): string | undefined {
		return this._contentKey;
	}
	public set contentKey(key: string | undefined) {
		if (!key || key === this._contentKey) return;
		this._contentKey = key;
		this._blockViewProps.contentKey = key;
		this.setAttribute('data-element-key', key);
		this.#context.setContentKey(key);

		new UmbObserveValidationStateController(
			this,
			`$.contentData[${UmbDataPathBlockElementDataQuery({ key: key })}]`,
			(hasMessages) => {
				this._contentInvalid = hasMessages;
				this._blockViewProps.contentInvalid = hasMessages;
			},
			'observeMessagesForContent',
		);
	}
	private _contentKey?: string | undefined;
	//

	#context = new UmbBlockGridEntryContext(this);
	#renderTimeout: number | undefined;

	@state()
	_contentTypeAlias?: string;

	@state()
	_contentTypeName?: string;

	@state()
	_columnSpan?: number;

	@state()
	_rowSpan?: number;

	@state()
	_showContentEdit = false;
	@state()
	_hasSettings = false;

	// If _createPath is undefined, its because no blocks are allowed to be created here[NL]
	@state()
	_createBeforePath?: string;
	@state()
	_createAfterPath?: string;

	@state()
	_label = '';

	@state()
	_icon?: string;

	@state()
	_exposed?: boolean;

	// Unuspported is triggerede if the Block Type is not reconized, it can also be triggerede by the Content Element Type not existing any longer. [NL]
	@state()
	_unsupported?: boolean;

	@state()
	_workspaceEditContentPath?: string;

	@state()
	_workspaceEditSettingsPath?: string;

	@state()
	_inlineEditingMode?: boolean;

	@state()
	_canScale?: boolean;
	@state()
	_showInlineCreateBefore?: boolean;
	@state()
	_showInlineCreateAfter?: boolean;
	@state()
	_inlineCreateAboveWidth?: string;

	// If the Block Type is disallowed in this location then it become a invalid Block Type. Notice not supported blocks are determined via the unsupported property. [NL]
	@property({ type: Boolean, attribute: 'location-invalid', reflect: true })
	_invalidLocation?: boolean;

	// 'content-invalid' attribute is used for styling purpose.
	@property({ type: Boolean, attribute: 'content-invalid', reflect: true })
	_contentInvalid?: boolean;

	// 'settings-invalid' attribute is used for styling purpose.
	@property({ type: Boolean, attribute: 'settings-invalid', reflect: true })
	_settingsInvalid?: boolean;

	@state()
	_blockViewProps: UmbBlockEditorCustomViewProperties<UmbBlockGridLayoutModel> = {
		contentKey: undefined!,
		config: { showContentEdit: false, showSettingsEdit: false },
	}; // Set to undefined cause it will be set before we render.

	#updateBlockViewProps(incoming: Partial<UmbBlockEditorCustomViewProperties<UmbBlockGridLayoutModel>>) {
		this._blockViewProps = { ...this._blockViewProps, ...incoming };
		this.requestUpdate('_blockViewProps');
	}

	@state()
	private _isReadOnly = false;

	constructor() {
		super();
		this.#init();
	}
	#init() {
		// Misc:
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
			this.#context.canScale,
			(canScale) => {
				this._canScale = canScale;
			},
			null,
		);
		this.observe(
			this.#context.isAllowed,
			(isAllowed) => {
				this._invalidLocation = !isAllowed;
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
		this.observe(
			this.#context.unsupported,
			(unsupported) => {
				if (unsupported === undefined) return;
				this.#updateBlockViewProps({ unsupported: unsupported });
				this._unsupported = unsupported;
				this.toggleAttribute('unsupported', unsupported);
			},
			null,
		);

		this.observe(
			this.#context.inlineEditingMode,
			(mode) => {
				this._inlineEditingMode = mode;
			},
			null,
		);

		// Data:
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

		// Paths:
		this.observe(
			this.#context.createBeforePath,
			(createPath) => {
				this._createBeforePath = createPath;
			},
			null,
		);
		this.observe(
			this.#context.createAfterPath,
			(createPath) => {
				this._createAfterPath = createPath;
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

		this.observe(
			this.#context.readOnlyState.isReadOnly,
			(isReadOnly) => (this._isReadOnly = isReadOnly),
			'umbReadonlyObserver',
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

	override connectedCallback(): void {
		super.connectedCallback();
		// element styling:
		this.observe(
			this.#context.columnSpan,
			(columnSpan) => {
				this._columnSpan = columnSpan;
				this.setAttribute('data-col-span', columnSpan ? columnSpan.toString() : '');
				this.style.setProperty('--umb-block-grid--item-column-span', columnSpan ? columnSpan.toString() : '');
			},
			'columnSpan',
		);
		this.observe(
			this.#context.rowSpan,
			(rowSpan) => {
				this._rowSpan = rowSpan;
				this.setAttribute('data-row-span', rowSpan ? rowSpan.toString() : '');
				this.style.setProperty('--umb-block-grid--item-row-span', rowSpan ? rowSpan.toString() : '');
			},
			'rowSpan',
		);
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
		this.observe(
			this.#context.contentElementTypeName,
			(contentElementTypeName) => {
				this._contentTypeName = contentElementTypeName;
			},
			'contentElementTypeName',
		);

		this.#callUpdateInlineCreateButtons();
	}

	protected override updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.updated(_changedProperties);
		if (_changedProperties.has('_blockViewProps') || _changedProperties.has('_columnSpan')) {
			this.#callUpdateInlineCreateButtons();
		}
	}

	#expose = () => {
		this.#context.expose();
	};

	#callUpdateInlineCreateButtons() {
		clearTimeout(this.#renderTimeout);
		this.#renderTimeout = setTimeout(this.#updateInlineCreateButtons, 100) as unknown as number;
	}

	#updateInlineCreateButtons = () => {
		// TODO: Could we optimize this, so it wont break?, cause currently we trust blindly that parentElement is '.umb-block-grid__layout-container' [NL]
		const layoutContainer = this.parentElement;
		if (!layoutContainer) return;
		const layoutContainerRect = layoutContainer.getBoundingClientRect();

		if (layoutContainerRect.width === 0) {
			this._showInlineCreateBefore = false;
			this._showInlineCreateAfter = false;
			this._inlineCreateAboveWidth = undefined;
			this.#renderTimeout = setTimeout(this.#updateInlineCreateButtons, 100) as unknown as number;
			return;
		}

		const layoutItemRect = this.getBoundingClientRect();
		if (layoutItemRect.right > layoutContainerRect.right - 5) {
			this._showInlineCreateAfter = false;
		} else {
			this._showInlineCreateAfter = true;
		}

		if (layoutItemRect.left > layoutContainerRect.left + 5) {
			this._showInlineCreateBefore = false;
			this._inlineCreateAboveWidth = undefined;
		} else {
			this._inlineCreateAboveWidth = getComputedStyle(layoutContainer).width;
			this._showInlineCreateBefore = true;
		}
	};

	#extensionSlotFilterMethod = (manifest: ManifestBlockEditorCustomView) => {
		if (!this._contentTypeAlias) {
			// accept no extensions if we don't have a content type alias
			return false;
		}
		// We do have _contentTypeAlias at this stage, cause we do use the filter method in the extension slot which first gets rendered when we have the _contentTypeAlias. [NL]
		if (
			manifest.forContentTypeAlias &&
			!stringOrStringArrayContains(manifest.forContentTypeAlias, this._contentTypeAlias!)
		) {
			return false;
		}
		if (manifest.forBlockEditor && !stringOrStringArrayContains(manifest.forBlockEditor, UMB_BLOCK_GRID)) {
			return false;
		}
		return true;
	};

	#extensionSlotRenderMethod = (ext: UmbExtensionElementInitializer<ManifestBlockEditorCustomView>) => {
		if (ext.component) {
			ext.component.classList.add('umb-block-grid__block--view');
		}
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

	override render() {
		return this.contentKey && (this._contentTypeAlias || this._unsupported)
			? html`
					${this.#renderCreateBeforeInlineButton()}
					<div class="umb-block-grid__block" part="umb-block-grid__block">
						<umb-extension-slot
							.filter=${this.#extensionSlotFilterMethod}
							.renderMethod=${this.#extensionSlotRenderMethod}
							.props=${this._blockViewProps}
							default-element=${this._inlineEditingMode ? 'umb-block-grid-block-inline' : 'umb-block-grid-block'}
							type="blockEditorCustomView"
							single
							>${this.#renderBuiltinBlockView()}</umb-extension-slot
						>
						${this.#renderActionBar()}
						${!this._showContentEdit && this._contentInvalid
							? html`<uui-badge attention color="invalid" label="Invalid content">!</uui-badge>`
							: nothing}
						${this._invalidLocation
							? html`<uui-tag id="invalidLocation" color="danger"
									><umb-localize key="blockEditor_invalidDropPosition" .args=${[this._label]}></umb-localize
								></uui-tag>`
							: nothing}
						${this._canScale
							? html` <umb-block-scale-handler
									@mousedown=${(e: MouseEvent) => this.#context.scaleManager.onScaleMouseDown(e)}>
									${this._columnSpan}x${this._rowSpan}
								</umb-block-scale-handler>`
							: nothing}
					</div>
					${this.#renderCreateAfterInlineButton()}
				`
			: nothing;
	}

	#renderBuiltinBlockView() {
		if (this._unsupported) {
			return this.#renderUnsupportedBlock();
		}
		if (this._inlineEditingMode) {
			return this.renderInlineBlock();
		}
		return this.#renderRefBlock();
	}

	#renderUnsupportedBlock() {
		return html`<umb-block-grid-block-unsupported
			class="umb-block-grid__block--view"
			.config=${this._blockViewProps.config}
			.content=${this._blockViewProps.content}
			.settings=${this._blockViewProps.settings}></umb-block-grid-block-unsupported>`;
		//TODO: investigate if we should have ${umbDestroyOnDisconnect()} here. Note how it works for drag n' drop in grid between areas and areas-root
	}

	renderInlineBlock() {
		return html`<umb-block-grid-block-inline
			class="umb-block-grid__block--view"
			.label=${this._label}
			.icon=${this._icon}
			.unpublished=${!this._exposed}
			.config=${this._blockViewProps.config}
			.content=${this._blockViewProps.content}
			.settings=${this._blockViewProps.settings}></umb-block-grid-block-inline>`;
		//TODO: investigate if we should have ${umbDestroyOnDisconnect()} here. Note how it works for drag n' drop in grid between areas and areas-root
	}

	#renderRefBlock() {
		return html`<umb-block-grid-block
			class="umb-block-grid__block--view"
			.label=${this._label}
			.icon=${this._icon}
			.unpublished=${!this._exposed}
			.config=${this._blockViewProps.config}
			.content=${this._blockViewProps.content}
			.settings=${this._blockViewProps.settings}></umb-block-grid-block>`;
		//TODO: investigate if we should have ${umbDestroyOnDisconnect()} here. Note how it works for drag n' drop in grid between areas and areas-root
	}

	#renderCreateBeforeInlineButton() {
		if (this._isReadOnly) return nothing;
		if (!this._createBeforePath) return nothing;
		if (!this._showInlineCreateBefore) return nothing;

		return html`<uui-button-inline-create
			href=${this._createBeforePath}
			label=${this.localize.term('blockEditor_addBlock')}
			style=${this._inlineCreateAboveWidth
				? `width: ${this._inlineCreateAboveWidth}`
				: ''}></uui-button-inline-create>`;
	}

	#renderCreateAfterInlineButton() {
		if (this._isReadOnly) return nothing;
		if (!this._createAfterPath) return nothing;
		if (!this._showInlineCreateAfter) return nothing;

		return html`
			<uui-button-inline-create
				vertical
				label=${this.localize.term('blockEditor_addBlock')}
				href=${this._createAfterPath}></uui-button-inline-create>
		`;
	}

	#renderActionBar() {
		return html`
			<uui-action-bar>
				${this.#renderEditAction()} ${this.#renderEditSettingsAction()} ${this.#renderCopyToClipboardAction()}
				${this.#renderDeleteAction()}</uui-action-bar
			>
		`;
	}

	#renderEditAction() {
		return this._showContentEdit && this._workspaceEditContentPath
			? html`<uui-button
					label="edit"
					look="secondary"
					color=${this._contentInvalid ? 'danger' : ''}
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
						look="secondary"
						><uui-icon name="icon-add"></uui-icon
					></uui-button>`
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

	#renderCopyToClipboardAction() {
		return html`<uui-button label="Copy to clipboard" look="secondary" @click=${() => this.#context.copyToClipboard()}>
			<uui-icon name="icon-clipboard-copy"></uui-icon>
		</uui-button>`;
	}

	#renderDeleteAction() {
		if (this._isReadOnly) return nothing;
		return html`
			<uui-button label="delete" look="secondary" @click=${() => this.#context.requestDelete()}>
				<uui-icon name="icon-remove"></uui-icon>
			</uui-button>
		`;
	}

	static override styles = [
		UUIBlinkKeyframes,
		css`
			:host {
				position: relative;
				display: block;
				--umb-block-grid-entry-actions-opacity: 0;
			}

			:host([settings-invalid]),
			:host([content-invalid]),
			:host(:hover),
			:host(:focus-within) {
				--umb-block-grid-entry-actions-opacity: 1;
			}

			:host::after {
				content: '';
				position: absolute;
				z-index: 1;
				pointer-events: none;
				inset: 0;
				border: 1px solid transparent;
				border-radius: var(--uui-border-radius);

				transition: border-color 240ms ease-in;
			}

			:host([location-invalid])::after,
			:host([settings-invalid])::after,
			:host([content-invalid])::after {
				border-color: var(--uui-color-invalid);
			}

			#invalidLocation {
				position: absolute;
				top: -1em;
				left: var(--uui-size-space-2);
				z-index: 2;
			}

			uui-action-bar {
				position: absolute;
				top: var(--uui-size-2);
				right: var(--uui-size-2);
				opacity: var(--umb-block-grid-entry-actions-opacity, 0);
				transition: opacity 120ms;
			}
			uui-button-inline-create {
				top: 0px;
				position: absolute;

				--umb-block-grid__block--inline-create-button-display--condition: var(--umb-block-grid--dragging-mode) none;
				display: var(--umb-block-grid__block--inline-create-button-display--condition);
			}
			uui-button-inline-create:not([vertical]) {
				left: 0;
				width: var(--umb-block-grid-editor--inline-create-width, 100%);
			}
			:host(:not([index='0'])) uui-button-inline-create:not([vertical]) {
				top: calc(var(--umb-block-grid--row-gap, 0px) * -0.5);
			}
			uui-button-inline-create[vertical] {
				right: calc(1px - (var(--umb-block-grid--column-gap, 0px) * 0.5));
			}

			.umb-block-grid__block {
				height: 100%;
			}

			:host(:hover):not(:drop)::after {
				display: block;
				border-color: var(--uui-color-interactive-emphasis);
				box-shadow:
					0 0 0 1px rgba(255, 255, 255, 0.7),
					inset 0 0 0 1px rgba(255, 255, 255, 0.7);
			}

			:host([drag-placeholder])::after {
				display: block;
				border-width: 2px;
				border-color: var(--uui-color-interactive-emphasis);
				animation: ${UUIBlinkAnimationValue};
			}
			:host([drag-placeholder])::before {
				content: '';
				position: absolute;
				pointer-events: none;
				inset: 0;
				border-radius: var(--uui-border-radius);
				background-color: var(--uui-color-interactive-emphasis);
				opacity: 0.12;
			}
			:host([drag-placeholder]) .umb-block-grid__block {
				transition: opacity 50ms 16ms;
				opacity: 0;
			}

			uui-badge {
				z-index: 2;
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
