import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbBlockGridEntryContext } from '../../context/block-grid-entry.context.js';
import { html, css, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import '../block-grid-block-view/index.js';
import type { UmbBlockGridLayoutModel, UmbBlockViewPropsType } from '@umbraco-cms/backoffice/block';

/**
 * @element umb-block-grid-entry
 */
@customElement('umb-block-grid-entry')
export class UmbBlockGridEntryElement extends UmbLitElement implements UmbPropertyEditorUiElement {
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
		this._blockViewProps.contentUdi = value;
		this.setAttribute('data-element-udi', value);
		this.#context.setContentUdi(value);
	}
	private _contentUdi?: string | undefined;
	//

	#context = new UmbBlockGridEntryContext(this);

	@state()
	_columnSpan?: number;

	@state()
	_rowSpan?: number;

	@state()
	_showContentEdit = false;
	@state()
	_hasSettings = false;
	@state()
	_createPath?: string;

	@state()
	_label = '';

	@state()
	_workspaceEditContentPath?: string;

	@state()
	_workspaceEditSettingsPath?: string;

	@state()
	_inlineEditingMode?: boolean;

	// TODO: use this type on the Element Interface for the Manifest.
	@state()
	_blockViewProps: UmbBlockViewPropsType<UmbBlockGridLayoutModel> = { contentUdi: undefined!, urls: {} }; // Set to undefined cause it will be set before we render.

	#updateBlockViewProps(incoming: Partial<UmbBlockViewPropsType<UmbBlockGridLayoutModel>>) {
		this._blockViewProps = { ...this._blockViewProps, ...incoming };
		this.requestUpdate('_blockViewProps');
	}

	constructor() {
		super();

		// Misc:
		this.observe(this.#context.showContentEdit, (showContentEdit) => {
			this._showContentEdit = showContentEdit;
		});
		this.observe(this.#context.settingsElementTypeKey, (settingsElementTypeKey) => {
			this._hasSettings = !!settingsElementTypeKey;
		});
		this.observe(this.#context.label, (label) => {
			this.#updateBlockViewProps({ label });
			this._label = label;
		});

		// Data:
		this.observe(this.#context.layout, (layout) => {
			this.#updateBlockViewProps({ layout });
		});
		this.observe(this.#context.content, (content) => {
			this.#updateBlockViewProps({ content });
		});
		this.observe(this.#context.settings, (settings) => {
			this.#updateBlockViewProps({ settings });
		});

		// Paths:
		this.observe(this.#context.createPath, (createPath) => {
			const oldValue = this._createPath;
			this._createPath = createPath;
			this.requestUpdate('_createPath', oldValue);
		});
		this.observe(this.#context.workspaceEditContentPath, (path) => {
			this._workspaceEditContentPath = path;
			this.#updateBlockViewProps({ urls: { ...this._blockViewProps.urls, editContent: path } });
		});
		this.observe(this.#context.workspaceEditSettingsPath, (path) => {
			this._workspaceEditSettingsPath = path;
			this.#updateBlockViewProps({ urls: { ...this._blockViewProps.urls, editSettings: path } });
		});
	}

	/*
	createRenderRoot() {
		return this;
	}
	*/

	connectedCallback(): void {
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
		this.observe(this.#context.contentElementTypeKey, (contentElementTypeKey) => {
			if (contentElementTypeKey) {
				this.setAttribute('data-content-element-type-key', contentElementTypeKey);
			}
		});
		this.observe(this.#context.contentElementTypeAlias, (contentElementTypeAlias) => {
			if (contentElementTypeAlias) {
				this.setAttribute('data-content-element-type-alias', contentElementTypeAlias);
			}
		});
	}

	#renderRefBlock() {
		return html`<umb-block-grid-block .contentUdi=${this.contentUdi} .label=${this._label}></umb-block-grid-block>`;
	}

	#renderBlock() {
		return this.contentUdi && this._createPath
			? html`
					<uui-button-inline-create href=${this._createPath}></uui-button-inline-create>
					<div class="umb-block-grid__block" part="umb-block-grid__block">
						<umb-extension-slot
							type="blockEditorCustomView"
							default-element=${'umb-block-grid-block'}
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
					</div>
			  `
			: '';
	}

	render() {
		return this.#renderBlock();
	}

	static styles = [
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

export default UmbBlockGridEntryElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-grid-entry': UmbBlockGridEntryElement;
	}
}
