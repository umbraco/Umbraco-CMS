import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import type { UmbInputDocumentElement } from '../../components/input-document/input-document.element.js';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyEditorUiInteractionMemoryManager } from '@umbraco-cms/backoffice/property-editor';
import type { UmbInteractionMemoryModel } from '@umbraco-cms/backoffice/interaction-memory';
import type { UmbNumberRangeValueType } from '@umbraco-cms/backoffice/models';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import type { UmbTreeStartNode } from '@umbraco-cms/backoffice/tree';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';

import '../../components/input-document/input-document.element.js';

/**
 * @element umb-property-editor-ui-multiple-document-picker
 */
@customElement('umb-property-editor-ui-multiple-document-picker')
export class UmbPropertyEditorUIMultipleDocumentPickerElement
	extends UmbFormControlMixin<Array<string> | undefined, typeof UmbLitElement, undefined>(UmbLitElement, undefined)
	implements UmbPropertyEditorUiElement
{
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this.#interactionMemoryManager.setPropertyEditorConfig(config);

		if (!config) return;

		const minMax = config.getValueByAlias<UmbNumberRangeValueType>('validationLimit');
		this._min = minMax?.min ?? 0;
		this._max = minMax?.max ?? Infinity;

		this._startNodeId = config.getValueByAlias('startNodeId');

		const allowedContentTypes = config.getValueByAlias<string>('allowedContentTypes');
		this._allowedContentTypes = allowedContentTypes ? allowedContentTypes.split(',').filter(Boolean) : undefined;
	}

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	@property({ type: Boolean })
	mandatory = false;

	@property({ type: String })
	mandatoryMessage = UMB_VALIDATION_EMPTY_LOCALIZATION_KEY;

	@state()
	private _min = 0;

	@state()
	private _max = Infinity;

	@state()
	private _startNodeId?: string;

	@state()
	private _allowedContentTypes?: Array<string>;

	@state()
	private _interactionMemories: Array<UmbInteractionMemoryModel> = [];

	#interactionMemoryManager = new UmbPropertyEditorUiInteractionMemoryManager(this, {
		memoryUniquePrefix: 'UmbMultipleDocumentPicker',
	});

	constructor() {
		super();

		this.observe(
			this.#interactionMemoryManager.memoriesForPropertyEditor,
			(interactionMemories) => {
				this._interactionMemories = interactionMemories ?? [];
			},
			null,
		);
	}

	override firstUpdated() {
		this.addFormControlElement(this.shadowRoot!.querySelector('umb-input-document')!);

		if (this._min && this._max && this._min > this._max) {
			console.warn(
				`Property (Multiple Document Picker) has been misconfigured, 'min' is greater than 'max'. Please correct your data type configuration.`,
				this,
			);
		}
	}

	override focus() {
		return this.shadowRoot?.querySelector<UmbInputDocumentElement>('umb-input-document')?.focus();
	}

	#onChange(event: CustomEvent & { target: UmbInputDocumentElement }) {
		const selection = event.target.selection;
		this.value = selection.length > 0 ? selection : undefined;
		this.dispatchEvent(new UmbChangeEvent());
	}

	async #onInputInteractionMemoriesChange(event: UmbChangeEvent) {
		const target = event.target as UmbInputDocumentElement;
		const interactionMemories = target.interactionMemories;

		if (interactionMemories && interactionMemories.length > 0) {
			await this.#interactionMemoryManager.saveMemoriesForPropertyEditor(interactionMemories);
		} else {
			await this.#interactionMemoryManager.deleteMemoriesForPropertyEditor();
		}
	}

	override render() {
		const startNode: UmbTreeStartNode | undefined = this._startNodeId
			? { unique: this._startNodeId, entityType: UMB_DOCUMENT_ENTITY_TYPE }
			: undefined;

		return html`
			<umb-input-document
				.min=${this._min}
				.max=${this._max}
				.startNode=${startNode}
				.allowedContentTypeIds=${this._allowedContentTypes}
				.selection=${this.value ?? []}
				@change=${this.#onChange}
				?readonly=${this.readonly}
				?required=${this.mandatory}
				.requiredMessage=${this.mandatoryMessage}
				.interactionMemories=${this._interactionMemories}
				@interaction-memories-change=${this.#onInputInteractionMemoriesChange}>
			</umb-input-document>
		`;
	}
}

export default UmbPropertyEditorUIMultipleDocumentPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-multiple-document-picker': UmbPropertyEditorUIMultipleDocumentPickerElement;
	}
}
