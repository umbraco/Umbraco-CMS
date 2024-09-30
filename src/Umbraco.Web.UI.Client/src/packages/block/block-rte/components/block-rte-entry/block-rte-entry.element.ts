import { UMB_BLOCK_RTE, type UmbBlockRteLayoutModel } from '../../types.js';
import { UmbBlockRteEntryContext } from '../../context/block-rte-entry.context.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { html, css, property, state, customElement } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/property-editor';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type {
	ManifestBlockEditorCustomView,
	UmbBlockEditorCustomViewProperties,
} from '@umbraco-cms/backoffice/block-custom-view';
import { stringOrStringArrayContains } from '@umbraco-cms/backoffice/utils';

import '../ref-rte-block/index.js';

/**
 * @class UmbBlockRteEntryElement
 */
@customElement('umb-rte-block')
export class UmbBlockRteEntryElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property({ type: String, attribute: 'data-content-udi', reflect: true })
	public get contentUdi(): string | undefined {
		return this.#contentUdi;
	}
	public set contentUdi(value: string | undefined) {
		if (!value) return;
		this.#contentUdi = value;
		this.#context.setContentUdi(value);
	}
	#contentUdi?: string;

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

	@state()
	_contentElementTypeAlias?: string;

	@state()
	_blockViewProps: UmbBlockEditorCustomViewProperties<UmbBlockRteLayoutModel> = {
		contentUdi: undefined!,
		config: { showContentEdit: false, showSettingsEdit: false },
	}; // Set to undefined cause it will be set before we render.

	#updateBlockViewProps(incoming: Partial<UmbBlockEditorCustomViewProperties<UmbBlockRteLayoutModel>>) {
		this._blockViewProps = { ...this._blockViewProps, ...incoming };
		this.requestUpdate('_blockViewProps');
	}

	constructor() {
		super();

		// We do not have index for RTE Blocks at the moment.
		this.#context.setIndex(0);

		this.observe(this.#context.showContentEdit, (showContentEdit) => {
			this._showContentEdit = showContentEdit;
			this.#updateBlockViewProps({ config: { ...this._blockViewProps.config, showContentEdit } });
		});
		this.observe(this.#context.settingsElementTypeKey, (key) => {
			this._hasSettings = !!key;
			this.#updateBlockViewProps({ config: { ...this._blockViewProps.config, showSettingsEdit: !!key } });
		});
		this.observe(this.#context.contentElementTypeAlias, (alias) => {
			this._contentElementTypeAlias = alias;
		});
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

	override connectedCallback() {
		super.connectedCallback();
		this.setAttribute('contenteditable', 'false');
	}

	#renderRefBlock() {
		return html`<umb-ref-rte-block .label=${this._label} .icon=${this._icon}></umb-ref-rte-block>`;
	}

	readonly #filterBlockCustomViews = (manifest: ManifestBlockEditorCustomView) => {
		const elementTypeAlias = this._contentElementTypeAlias ?? '';
		const isForBlockEditor =
			!manifest.forBlockEditor || stringOrStringArrayContains(manifest.forBlockEditor, UMB_BLOCK_RTE);
		const isForContentTypeAlias =
			!manifest.forContentTypeAlias || stringOrStringArrayContains(manifest.forContentTypeAlias, elementTypeAlias);
		return isForBlockEditor && isForContentTypeAlias;
	};

	#renderBlock() {
		return html`
			<div class="uui-text uui-font">
				<umb-extension-slot
					type="blockEditorCustomView"
					default-element="umb-ref-rte-block"
					.props=${this._blockViewProps}
					.filter=${this.#filterBlockCustomViews}
					single>
					${this.#renderRefBlock()}
				</umb-extension-slot>
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
				</uui-action-bar>
			</div>
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
