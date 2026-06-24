import type { UmbBlockRteLayoutModel } from '../../types.js';
import { UMB_BLOCK_RTE } from '../../constants.js';
import { UmbBlockRteEntryContext } from '../../context/block-rte-entry.context.js';
import { css, customElement, html, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { stringOrStringArrayContains, UmbDeprecation } from '@umbraco-cms/backoffice/utils';
import { UmbElementVariantState } from '@umbraco-cms/backoffice/element';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbDataPathBlockElementDataQuery } from '@umbraco-cms/backoffice/block';
import { UmbObserveValidationStateController } from '@umbraco-cms/backoffice/validation';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type {
	ManifestBlockEditorCustomView,
	UmbBlockEditorCustomViewProperties,
} from '@umbraco-cms/backoffice/block-custom-view';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/property-editor';
import type { UmbExtensionElementInitializer } from '@umbraco-cms/backoffice/extension-api';

import '../ref-rte-block/index.js';
import '../../../block/action/block-action-list.element.js';

/**
 * @class UmbBlockRteEntryElement
 */
@customElement('umb-rte-block')
export class UmbBlockRteEntryElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	/**
	 * The unique key of this block layout entry.
	 */
	@property({ type: String, attribute: 'data-key', reflect: true })
	public set key(value: string | undefined) {
		if (!value) return;
		this._key = value;
		this.#context.setKey(value);
	}
	public get key(): string | undefined {
		return this._key;
	}
	private _key?: string | undefined;

	/**
	 * @deprecated Use `key` instead. Will be removed in Umbraco 20.
	 */
	@property({ type: String, attribute: 'data-content-key', reflect: true })
	public set contentKey(value: string | undefined) {
		if (!value) return;
		new UmbDeprecation({
			deprecated: 'umb-rte-block.contentKey property',
			solution: 'Use the `key` property instead.',
			removeInVersion: '20.0.0',
		}).warn();
		this._contentKey = value;
		if (!this._key) {
			this.#context.setKey(value);
		}
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
	public get contentKey(): string | undefined {
		return this._contentKey;
	}
	private _contentKey?: string | undefined;

	#context = new UmbBlockRteEntryContext(this);

	@state()
	private _showContentEdit = false;

	@state()
	private _label = '';

	@state()
	private _icon?: string;

	@state()
	private _exposed?: boolean;

	private _localExpose?: boolean;

	@state()
	private _externalContentVariantState: string | null | undefined;

	@state()
	private _showActions?: boolean;

	@state()
	private _contentTypeAlias?: string;

	@state()
	private _contentTypeName?: string;

	@state()
	private _isReadOnly = false;

	@state()
	private _blockViewProps: UmbBlockEditorCustomViewProperties<UmbBlockRteLayoutModel> = {
		contentKey: undefined!,
		config: { showContentEdit: false, showSettingsEdit: false },
	}; // Set to undefined cause it will be set before we render.

	// 'is-reference' attribute is used for styling purpose.
	@property({ type: Boolean, attribute: 'is-reference', reflect: true })
	private _isExternalContent = false;

	// 'content-invalid' attribute is used for styling purpose.
	@property({ type: Boolean, attribute: 'content-invalid', reflect: true })
	// eslint-disable-next-line @typescript-eslint/naming-convention
	_contentInvalid?: boolean;

	// 'settings-invalid' attribute is used for styling purpose.
	@property({ type: Boolean, attribute: 'settings-invalid', reflect: true })
	// eslint-disable-next-line @typescript-eslint/naming-convention
	_settingsInvalid?: boolean;

	#updateExposedState() {
		// External content blocks use the element's variant state; local blocks use the expose entry
		const isExposed = this._isExternalContent
			? this._externalContentVariantState === UmbElementVariantState.PUBLISHED ||
				this._externalContentVariantState === UmbElementVariantState.PUBLISHED_PENDING_CHANGES
			: this._localExpose;
		this.#updateBlockViewProps({ unpublished: !isExposed });
		this._exposed = isExposed;
	}

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
		this.observe(this.#context.index, (index) => this.#updateBlockViewProps({ index }), null);
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
				this._localExpose = exposed;
				this.#updateExposedState();
			},
			null,
		);

		this.observe(
			this.#context.isExternalContent,
			(isExternalContent) => {
				this._isExternalContent = isExternalContent ?? false;
				this.#updateExposedState();
			},
			null,
		);

		this.observe(
			this.#context.externalContentVariantState,
			(state) => {
				this._externalContentVariantState = state;
				this.#updateExposedState();
			},
			null,
		);

		this.observe(this.#context.actionsVisibility, (showActions) => (this._showActions = showActions), null);

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
				this.#updateBlockViewProps({ config: { ...this._blockViewProps.config!, editContentPath: path } });
			},
			null,
		);
		this.observe(
			this.#context.workspaceEditSettingsPath,
			(path) => {
				this.#updateBlockViewProps({ config: { ...this._blockViewProps.config!, editSettingsPath: path } });
			},
			null,
		);
		this.observe(
			this.#context.readOnlyGuard.permitted,
			(isReadOnly) => {
				this._isReadOnly = isReadOnly;
				this.#updateBlockViewProps({ readonly: isReadOnly });
			},
			'umbReadOnlyObserver',
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
		if (this._exposed || this._isReadOnly) {
			return ext.component;
		} else {
			return html`
				<div>
					${ext.component}
					<umb-block-overlay-expose-button
						.contentTypeName=${this._contentTypeName}
						@click=${this.#expose}></umb-block-overlay-expose-button>
				</div>
			`;
		}
	};

	#renderBlock() {
		return this.contentKey && this._contentTypeAlias
			? html`
					<div class="umb-block-rte__block uui-text uui-font">
						<umb-entity-frame .label=${this._label}></umb-entity-frame>
						<umb-extension-slot
							type="blockEditorCustomView"
							default-element="umb-ref-rte-block"
							.renderMethod=${this.#extensionSlotRenderMethod}
							.fallbackRenderMethod=${this.#renderBuiltinBlockView}
							.props=${this._blockViewProps}
							.filter=${this.#filterBlockCustomViews}
							single></umb-extension-slot>
						${this.#renderActionBar()}
						${!this._showContentEdit && this._contentInvalid
							? html`<uui-badge attention color="invalid" label="Invalid content">!</uui-badge>`
							: nothing}
					</div>
				`
			: nothing;
	}

	#renderActionBar() {
		if (!this._showActions) return nothing;
		return html`<umb-block-action-list id="actions" .blockEditor=${UMB_BLOCK_RTE}></umb-block-action-list>`;
	}

	#renderBuiltinBlockView = () => {
		// TODO: Missing unsupported rendering [NL]
		/*if (this._unsupported) {
			return this.#renderUnsupportedBlock();
		}*/
		return this.#renderRefBlock();
	};

	#renderRefBlock() {
		return html`
			<umb-ref-rte-block
				.label=${this._label}
				.icon=${this._icon}
				.index=${this._blockViewProps.index}
				.unpublished=${!this._exposed}
				.content=${this._blockViewProps.content}
				.settings=${this._blockViewProps.settings}
				.config=${this._blockViewProps.config}>
			</umb-ref-rte-block>
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
				user-select: all;
				user-drag: auto;
				white-space: nowrap;

				--umb-block-entry-actions-opacity: 0;
			}

			:host(:hover),
			:host(:focus-within) {
				--umb-block-entry-actions-opacity: 1;
			}

			:host(.ProseMirror-selectednode) {
				umb-ref-rte-block {
					--uui-color-default-contrast: initial;
					outline: 3px solid var(--uui-color-focus);
				}
			}

			umb-extension-slot::part(component) {
				position: relative;
				z-index: 0;
			}

			#actions {
				position: absolute;
				top: var(--uui-size-2);
				right: var(--uui-size-2);
				opacity: var(--umb-block-entry-actions-opacity, 0);
				transition: opacity 120ms;
				z-index: 1;
			}

			:host([drag-placeholder]) {
				opacity: 0.2;
			}

			:host([is-reference]) .umb-block-rte__block {
				--umb-entity-frame-color: var(--umb-color-reference, #7532c8);
				--umb-entity-frame-contrast-color: var(--umb-color-reference-contrast, #ffffff);
			}

			.umb-block-rte__block {
				--umb-entity-frame-opacity: 0;
				--umb-entity-frame-color: var(--uui-color-interactive-emphasis);

				&:hover,
				&:focus-within {
					--umb-entity-frame-opacity: 1;
				}
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
