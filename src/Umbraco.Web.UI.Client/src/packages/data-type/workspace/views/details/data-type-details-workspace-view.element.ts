import { UMB_DATA_TYPE_WORKSPACE_CONTEXT } from '../../data-type-workspace.context-token.js';
import { css, customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UMB_PROPERTY_EDITOR_UI_PICKER_MODAL } from '@umbraco-cms/backoffice/property-editor';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';
import { umbBindToValidation } from '@umbraco-cms/backoffice/validation';

@customElement('umb-data-type-details-workspace-view')
export class UmbDataTypeDetailsWorkspaceViewEditElement extends UmbLitElement implements UmbWorkspaceViewElement {
	@state()
	private _propertyEditorUiIcon?: string | null = null;

	@state()
	private _propertyEditorUiName?: string | null = null;

	@state()
	private _propertyEditorUiAlias?: string | null = null;

	@state()
	private _propertyEditorSchemaAlias?: string | null = null;

	#workspaceContext?: typeof UMB_DATA_TYPE_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_DATA_TYPE_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#workspaceContext = workspaceContext;
			this.#observeDataType();
		});
	}

	#observeDataType() {
		if (!this.#workspaceContext) {
			return;
		}

		this.observe(this.#workspaceContext.propertyEditorUiAlias, (value) => {
			this._propertyEditorUiAlias = value;
		});

		this.observe(this.#workspaceContext.propertyEditorSchemaAlias, (value) => {
			this._propertyEditorSchemaAlias = value;
		});

		this.observe(this.#workspaceContext.propertyEditorUiName, (value) => {
			this._propertyEditorUiName = value;
		});

		this.observe(this.#workspaceContext.propertyEditorUiIcon, (value) => {
			this._propertyEditorUiIcon = value;
		});
	}

	async #openPropertyEditorUIPicker() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const value = await modalManager
			.open(this, UMB_PROPERTY_EDITOR_UI_PICKER_MODAL, {
				value: {
					selection: this._propertyEditorUiAlias ? [this._propertyEditorUiAlias] : [],
				},
			})
			.onSubmit()
			.catch(() => undefined);

		if (value) {
			this.#workspaceContext?.setPropertyEditorUiAlias(value.selection[0]);
		}
	}

	override render() {
		return html`
			<uui-box>
				${this._propertyEditorUiAlias && this._propertyEditorSchemaAlias
					? this.#renderPropertyEditorReference()
					: this.#renderChooseButton()}
			</uui-box>
			${this.#renderSettings()}
		`;
	}

	#renderSettings() {
		if (!this._propertyEditorUiAlias || !this._propertyEditorSchemaAlias) return nothing;
		return html`
			<uui-box headline=${this.localize.term('general_settings')}>
				<umb-property-editor-config></umb-property-editor-config>
			</uui-box>
		`;
	}

	// Notice, we have implemented a property-layout for each states of the property editor ui picker, in this way the validation message gets removed once the choose-button is gone. (As we are missing ability to detect if elements got removed from DOM)[NL]
	#renderChooseButton() {
		return html`
			<umb-property-layout
				data-mark="property:editorUiAlias"
				label="Property Editor"
				description=${this.localize.term('propertyEditorPicker_title')}>
				<uui-button
					slot="editor"
					id="btn-add"
					label=${this.localize.term('propertyEditorPicker_title')}
					look="placeholder"
					color="default"
					required
					${umbBindToValidation(this)}
					@click=${this.#openPropertyEditorUIPicker}></uui-button>
			</umb-property-layout>
		`;
	}

	#renderPropertyEditorReference() {
		if (!this._propertyEditorUiAlias || !this._propertyEditorSchemaAlias) return nothing;
		return html`
			<umb-property-layout
				data-mark="property:editorUiAlias"
				label="Property Editor"
				description=${this.localize.term('propertyEditorPicker_title')}>
				<umb-ref-property-editor-ui
					slot="editor"
					name=${this._propertyEditorUiName ?? ''}
					alias=${this._propertyEditorUiAlias}
					property-editor-schema-alias=${this._propertyEditorSchemaAlias}
					standalone
					@open=${this.#openPropertyEditorUIPicker}>
					${this._propertyEditorUiIcon
						? html`<umb-icon name=${this._propertyEditorUiIcon} slot="icon"></umb-icon>`
						: nothing}
					<uui-action-bar slot="actions">
						<uui-button
							label=${this.localize.term('general_change')}
							@click=${this.#openPropertyEditorUIPicker}></uui-button>
					</uui-action-bar>
				</umb-ref-property-editor-ui>
			</umb-property-layout>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				margin: var(--uui-size-layout-1);
				padding-bottom: var(--uui-size-layout-1);
			}

			uui-box {
				margin-top: var(--uui-size-layout-1);
			}

			#btn-add {
				display: block;
			}
		`,
	];
}

export default UmbDataTypeDetailsWorkspaceViewEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-data-type-details-workspace-view': UmbDataTypeDetailsWorkspaceViewEditElement;
	}
}
