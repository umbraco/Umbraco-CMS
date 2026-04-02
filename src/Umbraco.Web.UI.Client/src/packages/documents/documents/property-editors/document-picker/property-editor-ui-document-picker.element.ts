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

@customElement('umb-property-editor-ui-document-picker')
export class UmbPropertyEditorUIDocumentPickerElement
	extends UmbFormControlMixin<string, typeof UmbLitElement, undefined>(UmbLitElement)
	implements UmbPropertyEditorUiElement
{
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this.#interactionMemoryManager.setPropertyEditorConfig(config);

		if (!config) return;

		const minMax = config.getValueByAlias<UmbNumberRangeValueType>('validationLimit');
		if (minMax) {
			this._min = minMax.min && minMax.min > 0 ? minMax.min : 0;
			this._max = minMax.max && minMax.max > 0 ? minMax.max : 1;
		}

		this._startNodeId = config.getValueByAlias('startNodeId');
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

	// NOTE: The legacy "Content Picker" property-editor only supported 1 item,
	// so that's why it's being enforced here. We'll evolve this in a future version. [LK]
	@state()
	private _max = 1;

	@state()
	private _startNodeId?: string;

	@state()
	private _interactionMemories: Array<UmbInteractionMemoryModel> = [];

	#interactionMemoryManager = new UmbPropertyEditorUiInteractionMemoryManager(this, {
		memoryUniquePrefix: 'UmbDocumentPicker',
	});

	constructor() {
		super();

		this.observe(this.#interactionMemoryManager.memoriesForPropertyEditor, (interactionMemories) => {
			this._interactionMemories = interactionMemories ?? [];
		});
	}

	override firstUpdated() {
		this.addFormControlElement(this.shadowRoot!.querySelector('umb-input-document')!);
	}

	#onChange(event: CustomEvent & { target: UmbInputDocumentElement }) {
		this.value = event.target.value;
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
				.value=${this.value}
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

export default UmbPropertyEditorUIDocumentPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-document-picker': UmbPropertyEditorUIDocumentPickerElement;
	}
}
