import { UmbBlockGridEntryContext } from '../../context/block-grid-entry.context.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { html, css, customElement, property, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import type {
	ManifestBlockEditorCustomView,
	UmbBlockEditorCustomViewProperties,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/extension-registry';
import { stringOrStringArrayContains } from '@umbraco-cms/backoffice/utils';
import { UMB_BLOCK_GRID, type UmbBlockGridLayoutModel } from '@umbraco-cms/backoffice/block-grid';

import '../block-grid-block-inline/index.js';
import '../block-grid-block/index.js';
import '../block-scale-handler/index.js';
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
	public get contentUdi(): string | undefined {
		return this._contentUdi;
	}
	public set contentUdi(value: string | undefined) {
		if (!value || value === this._contentUdi) return;
		this._contentUdi = value;
		this._blockViewProps.contentUdi = value;
		this.setAttribute('data-element-udi', value);
		this.#context.setContentUdi(value);
	}
	private _contentUdi?: string | undefined;
	//

	#context = new UmbBlockGridEntryContext(this);
	#renderTimeout: number | undefined;

	@state()
	_contentTypeAlias?: string;

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

	@state()
	_blockViewProps: UmbBlockEditorCustomViewProperties<UmbBlockGridLayoutModel> = {
		contentUdi: undefined!,
		config: { showContentEdit: false, showSettingsEdit: false },
	}; // Set to undefined cause it will be set before we render.

	#updateBlockViewProps(incoming: Partial<UmbBlockEditorCustomViewProperties<UmbBlockGridLayoutModel>>) {
		this._blockViewProps = { ...this._blockViewProps, ...incoming };
		this.requestUpdate('_blockViewProps');
	}

	constructor() {
		super();

		// Misc:
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
			this.#context.canScale,
			(canScale) => {
				this._canScale = canScale;
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
			},
			null,
		);

		// Paths:
		this.observe(
			this.#context.createBeforePath,
			(createPath) => {
				//const oldValue = this._createBeforePath;
				this._createBeforePath = createPath;
				//this.requestUpdate('_createPath', oldValue);
			},
			null,
		);
		this.observe(
			this.#context.createAfterPath,
			(createPath) => {
				//const oldValue = this._createAfterPath;
				this._createAfterPath = createPath;
				//this.requestUpdate('_createPath', oldValue);
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

		this.#callUpdateInlineCreateButtons();
	}

	protected override updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.updated(_changedProperties);
		if (_changedProperties.has('_blockViewProps') || _changedProperties.has('_columnSpan')) {
			this.#callUpdateInlineCreateButtons();
		}
	}

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

	#renderInlineEditBlock() {
		return html`<umb-block-grid-block-inline .label=${this._label}></umb-block-grid-block-inline>`;
	}

	#renderRefBlock() {
		return html`<umb-block-grid-block .label=${this._label}></umb-block-grid-block>`;
	}

	#renderBlock() {
		return this.contentUdi && this._contentTypeAlias
			? html`
					${this._createBeforePath && this._showInlineCreateBefore
						? html`<uui-button-inline-create
								href=${this._createBeforePath}
								label=${this.localize.term('blockEditor_addBlock')}
								style=${this._inlineCreateAboveWidth
									? `width: ${this._inlineCreateAboveWidth}`
									: ''}></uui-button-inline-create>`
						: nothing}
					<div class="umb-block-grid__block" part="umb-block-grid__block">
						<umb-extension-slot
							type="blockEditorCustomView"
							default-element="umb-block-grid-block"
							.props=${this._blockViewProps}
							.filter=${this.#extensionSlotFilterMethod}
							>${this._inlineEditingMode ? this.#renderInlineEditBlock() : this.#renderRefBlock()}</umb-extension-slot
						>
						<uui-action-bar>
							${this._showContentEdit && this._workspaceEditContentPath
								? html`<uui-button label="edit" compact href=${this._workspaceEditContentPath}>
										<uui-icon name="icon-edit"></uui-icon>
									</uui-button>`
								: nothing}
							${this._hasSettings && this._workspaceEditSettingsPath
								? html`<uui-button label="Edit settings" compact href=${this._workspaceEditSettingsPath}>
										<uui-icon name="icon-settings"></uui-icon>
									</uui-button>`
								: nothing}
							<uui-button label="delete" compact @click=${() => this.#context.requestDelete()}>
								<uui-icon name="icon-remove"></uui-icon>
							</uui-button>
						</uui-action-bar>

						${this._canScale
							? html` <umb-block-scale-handler
									@mousedown=${(e: MouseEvent) => this.#context.scaleManager.onScaleMouseDown(e)}>
									${this._columnSpan}x${this._rowSpan}
								</umb-block-scale-handler>`
							: nothing}
					</div>
					${this._createAfterPath && this._showInlineCreateAfter
						? html`<uui-button-inline-create
								vertical
								label=${this.localize.term('blockEditor_addBlock')}
								href=${this._createAfterPath}></uui-button-inline-create>`
						: nothing}
				`
			: nothing;
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
			uui-button-inline-create {
				top: 0px;
				position: absolute;

				// Avoid showing inline-create in dragging-mode
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

			:host([drag-placeholder]) {
				opacity: 0.2;
			}

			:host(::after) {
				content: '';
				position: absolute;
				z-index: 1;
				pointer-events: none;
				display: none;
				inset: 0;
				border: 1px solid transparent;
				border-radius: 3px;
				box-shadow:
					0 0 0 1px rgba(255, 255, 255, 0.7),
					inset 0 0 0 1px rgba(255, 255, 255, 0.7);

				transition: border-color 240ms ease-in;
			}

			:host(:hover::after) {
				// TODO: Look at the feature I out-commented here, what was that suppose to do [NL]:
				//display: var(--umb-block-grid--block-ui-display, block);
				display: block;
				border-color: var(--uui-color-interactive);
			}

			.umb-block-grid__block {
				height: 100%;
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
