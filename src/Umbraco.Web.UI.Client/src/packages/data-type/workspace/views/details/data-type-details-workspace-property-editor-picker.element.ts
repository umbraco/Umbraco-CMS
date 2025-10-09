import { UMB_DATA_TYPE_WORKSPACE_CONTEXT } from '../../data-type-workspace.context-token.js';
import { css, customElement, html, nothing, property, ref } from '@umbraco-cms/backoffice/external/lit';
import type { UUIButtonElement } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import {
	UMB_MISSING_PROPERTY_EDITOR_UI_ALIAS,
	UMB_PROPERTY_EDITOR_UI_PICKER_MODAL,
} from '@umbraco-cms/backoffice/property-editor';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';

/**
 * @internal
 */
@customElement('umb-data-type-details-workspace-property-editor-picker')
export class UmbDataTypeDetailsWorkspacePropertyEditorPickerElement extends UmbFormControlMixin<
	undefined,
	typeof UmbLitElement,
	undefined
>(UmbLitElement) {
	@property({ type: String })
	propertyEditorUiIcon?: string;

	@property({ type: String })
	propertyEditorUiName?: string;

	@property({ type: String })
	propertyEditorUiAlias?: string;

	@property({ type: String })
	propertyEditorSchemaAlias?: string;

	#workspaceContext?: typeof UMB_DATA_TYPE_WORKSPACE_CONTEXT.TYPE;
	#addButton?: UUIButtonElement;

	constructor() {
		super();

		this.consumeContext(UMB_DATA_TYPE_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#workspaceContext = workspaceContext;
		});

		this.addValidator(
			'customError',
			() => this.localize.term('missingEditor_dataTypeMissingEditorUiMessage'),
			() => !this.propertyEditorUiName,
		);

		this.addValidator(
			'customError',
			() => this.localize.term('missingEditor_dataTypeMissingEditorMessage'),
			() => this.propertyEditorUiAlias === UMB_MISSING_PROPERTY_EDITOR_UI_ALIAS,
		);
	}

	protected override getFormElement() {
		return undefined;
	}

	#addButtonRefChanged(input?: Element) {
		if (this.#addButton) {
			this.removeFormControlElement(this.#addButton);
		}
		this.#addButton = input as UUIButtonElement | undefined;
		if (this.#addButton) {
			this.addFormControlElement(this.#addButton);
		}
	}

	async #openPropertyEditorUIPicker() {
		const value = await umbOpenModal(this, UMB_PROPERTY_EDITOR_UI_PICKER_MODAL, {
			value: {
				selection: this.propertyEditorUiAlias ? [this.propertyEditorUiAlias] : [],
			},
		}).catch(() => undefined);

		if (value) {
			this.#workspaceContext?.setPropertyEditorUiAlias(value.selection[0]);
		}
	}

	#renderPropertyEditorReference() {
		if (!this.propertyEditorUiAlias || !this.propertyEditorSchemaAlias) return nothing;

		let name = this.propertyEditorUiName;
		let alias = this.propertyEditorUiAlias;
		let error = false;

		if (!this.propertyEditorUiName) {
			name = this.localize.term('missingEditor_dataTypeMissingEditorUi');
			error = true;
		}

		if (this.propertyEditorUiAlias === UMB_MISSING_PROPERTY_EDITOR_UI_ALIAS) {
			name = this.localize.term('missingEditor_dataTypeMissingEditor');
			alias = '';
			error = true;
		}

		return html`
			<umb-ref-property-editor-ui
				name=${name ?? ''}
				alias=${alias}
				property-editor-schema-alias=${this.propertyEditorSchemaAlias}
				standalone
				?error=${error}
				@open=${this.#openPropertyEditorUIPicker}>
				${this.propertyEditorUiIcon
					? html`<umb-icon name=${this.propertyEditorUiIcon} slot="icon"></umb-icon>`
					: nothing}
				<uui-action-bar slot="actions">
					<uui-button
						label=${this.localize.term('general_change')}
						@click=${this.#openPropertyEditorUIPicker}></uui-button>
				</uui-action-bar>
			</umb-ref-property-editor-ui>
		`;
	}

	#renderChooseButton() {
		return html`
			<uui-button
				id="btn-add"
				label=${this.localize.term('propertyEditorPicker_title')}
				look="placeholder"
				color="default"
				required
				@click=${this.#openPropertyEditorUIPicker}
				${ref(this.#addButtonRefChanged)}></uui-button>
		`;
	}

	override render() {
		return html`
			${this.propertyEditorUiAlias && this.propertyEditorSchemaAlias
				? this.#renderPropertyEditorReference()
				: this.#renderChooseButton()}
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#btn-add {
				display: block;
			}
		`,
	];
}

export default UmbDataTypeDetailsWorkspacePropertyEditorPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-data-type-details-workspace-property-editor-picker': UmbDataTypeDetailsWorkspacePropertyEditorPickerElement;
	}
}
