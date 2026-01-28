import type { UmbBlockRteLayoutModel, UmbBlockRteValueModel } from '../../types.js';
import { UMB_BLOCK_RTE, UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../constants.js';
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
import { UMB_PROPERTY_CONTEXT, UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import { UMB_CLIPBOARD_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/clipboard';

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
	public get contentKey(): string | undefined {
		return this._contentKey;
	}
	private _contentKey?: string | undefined;

	#context = new UmbBlockRteEntryContext(this);

	@state()
	private _showContentEdit = false;

	@state()
	private _hasSettings = false;

	@state()
	private _label = '';

	@state()
	private _icon?: string;

	@state()
	private _exposed?: boolean;

	@state()
	private _showActions?: boolean;

	@state()
	private _workspaceEditContentPath?: string;

	@state()
	private _workspaceEditSettingsPath?: string;

	@state()
	private _contentTypeAlias?: string;

	@state()
	private _contentTypeName?: string;

	@state()
	private _blockViewProps: UmbBlockEditorCustomViewProperties<UmbBlockRteLayoutModel> = {
		contentKey: undefined!,
		config: { showContentEdit: false, showSettingsEdit: false },
	}; // Set to undefined cause it will be set before we render.

	// 'content-invalid' attribute is used for styling purpose.
	@property({ type: Boolean, attribute: 'content-invalid', reflect: true })
	// eslint-disable-next-line @typescript-eslint/naming-convention
	_contentInvalid?: boolean;

	// 'settings-invalid' attribute is used for styling purpose.
	@property({ type: Boolean, attribute: 'settings-invalid', reflect: true })
	// eslint-disable-next-line @typescript-eslint/naming-convention
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
				this.#updateBlockViewProps({ unpublished: !exposed });
				this._exposed = exposed;
			},
			null,
		);

		this.observe(
			this.#context.actionsVisibility,
			(showActions) => {
				this._showActions = showActions;
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

	async #copyToClipboard() {
		const contexts = await this.#getCopyContexts();
		const entryName = this.#buildEntryName(contexts);
		const propertyValue = this.#buildPropertyValue();

		contexts.clipboardContext.write({
			icon: this._icon,
			name: entryName,
			propertyValue,
			propertyEditorUiAlias: 'Umb.PropertyEditorUi.RichText',
		});
	}

	async #getCopyContexts() {
		const propertyDatasetContext = await this.getContext(UMB_PROPERTY_DATASET_CONTEXT);
		if (!propertyDatasetContext) {
			throw new Error('Property dataset context not found.');
		}

		const propertyContext = await this.getContext(UMB_PROPERTY_CONTEXT);
		if (!propertyContext) {
			throw new Error('Property context not found.');
		}

		const clipboardContext = await this.getContext(UMB_CLIPBOARD_PROPERTY_CONTEXT);
		if (!clipboardContext) {
			throw new Error('Clipboard context not found.');
		}

		return { propertyDatasetContext, propertyContext, clipboardContext };
	}

	#buildEntryName(contexts: {
		propertyDatasetContext: { getName(): string };
		propertyContext: { getLabel(): string };
		clipboardContext: { write(data: unknown): void };
	}) {
		const workspaceName = this.localize.string(contexts.propertyDatasetContext.getName());
		const propertyLabel = this.localize.string(contexts.propertyContext.getLabel());
		const blockLabel = this.#context.getName();

		if (workspaceName) {
			return `${workspaceName} - ${propertyLabel} - ${blockLabel}`;
		}
		return `${propertyLabel} - ${blockLabel}`;
	}

	#buildPropertyValue(): UmbBlockRteValueModel {
		const content = this.#context.getContent();
		const layout = this.#context.getLayout();
		const settings = this.#context.getSettings();
		const expose = this.#context.getExpose();

		const contentData = content ? [structuredClone(content)] : [];
		const layoutData = layout ? [structuredClone(layout)] : undefined;
		const settingsData = settings ? [structuredClone(settings)] : [];
		const exposeData = expose ? [structuredClone(expose)] : [];

		return {
			contentData,
			layout: {
				[UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS]: layoutData,
			},
			settingsData,
			expose: exposeData,
		};
	}

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
		return this._showActions
			? html`<uui-action-bar>${this.#renderEditAction()}${this.#renderEditSettingsAction()}${this.#renderCopyToClipboardAction()}${this.#renderDeleteAction()}</uui-action-bar>`
			: nothing;
	}

	#renderBuiltinBlockView = () => {
		// TODO: Missing unsupported rendering [NL]
		/*if (this._unsupported) {
			return this.#renderUnsupportedBlock();
		}*/
		return this.#renderRefBlock();
	};

	#renderRefBlock() {
		return html`<umb-ref-rte-block
			.label=${this._label}
			.icon=${this._icon}
			.index=${this._blockViewProps.index}
			.unpublished=${!this._exposed}
			.content=${this._blockViewProps.content}
			.settings=${this._blockViewProps.settings}
			.config=${this._blockViewProps.config}></umb-ref-rte-block>`;
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

	#renderDeleteAction() {
		return html`<uui-button label="delete" look="secondary" @click=${() => this.#context.requestDelete()}>
			<uui-icon name="icon-remove"></uui-icon>
		</uui-button>`;
	}

	#renderCopyToClipboardAction() {
		return html`<uui-button label="Copy to clipboard" look="secondary" @click=${() => this.#copyToClipboard()}>
			<uui-icon name="icon-clipboard-copy"></uui-icon>
		</uui-button>`;
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
