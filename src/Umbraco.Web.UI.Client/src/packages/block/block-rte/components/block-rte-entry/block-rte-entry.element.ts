import type { UmbBlockRteLayoutModel } from '../../types.js';
import { UmbBlockRteEntryContext } from '../../context/block-rte-entry.context.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { html, css, property, state, customElement } from '@umbraco-cms/backoffice/external/lit';
import type {
	UmbBlockEditorCustomViewProperties,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/extension-registry';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

import '../ref-rte-block/index.js';

/**
 * @class UmbBlockRteEntryElement
 */
@customElement('umb-rte-block')
export class UmbBlockRteEntryElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	//
	@property({ type: String, attribute: 'data-content-key', reflect: true })
	public get contentKey(): string | undefined {
		return this._contentKey;
	}
	public set contentKey(value: string | undefined) {
		if (!value) return;
		this._contentKey = value;
		this.#context.setContentKey(value);
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
	_workspaceEditContentPath?: string;

	@state()
	_workspaceEditSettingsPath?: string;

	@state()
	_blockViewProps: UmbBlockEditorCustomViewProperties<UmbBlockRteLayoutModel> = {
		contentKey: undefined!,
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
			this.#context.contentValues,
			(content) => {
				this.#updateBlockViewProps({ content });
			},
			null,
		);
		this.observe(
			this.#context.settingsValues,
			(settings) => {
				this.#updateBlockViewProps({ settings });
			},
			null,
		);
		// TODO: Implement validation for RTE Blocks: [NL]
		/*
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
		*/
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

	#renderBlock() {
		return html`
			<div class="uui-text uui-font">
				<umb-extension-slot
					type="blockEditorCustomView"
					default-element=${'umb-ref-rte-block'}
					.props=${this._blockViewProps}
					single
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
			</div>
		`;
	}

	override render() {
		return this.#renderBlock();
	}

	static override styles = [
		UmbTextStyles,
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
