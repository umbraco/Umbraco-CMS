import type { UmbInputDocumentElement } from '../../components/input-document/input-document.element.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbNumberRangeValueType } from '@umbraco-cms/backoffice/models';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import type { UmbTreeStartNode } from '@umbraco-cms/backoffice/tree';
import {
	UMB_INTERACTION_MEMORY_CONTEXT,
	type UmbInteractionMemoryModel,
} from '@umbraco-cms/backoffice/interaction-memory';
import { simpleHashCode } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-property-editor-ui-document-picker')
export class UmbPropertyEditorUIDocumentPickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	public value?: string;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this.#setConfigHash(config);

		if (!config) return;

		const minMax = config.getValueByAlias<UmbNumberRangeValueType>('validationLimit');
		if (minMax) {
			this._min = minMax.min && minMax.min > 0 ? minMax.min : 0;
			this._max = minMax.max && minMax.max > 0 ? minMax.max : 1;
		}

		this._startNodeId = config.getValueByAlias('startNodeId');

		this.#getInteractionMemory();
	}

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

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

	#interactionMemoryContext?: typeof UMB_INTERACTION_MEMORY_CONTEXT.TYPE;
	#configHashCode?: number;

	constructor() {
		super();

		this.consumeContext(UMB_INTERACTION_MEMORY_CONTEXT, (context) => {
			this.#interactionMemoryContext = context;
			this.#getInteractionMemory();
		});
	}

	#setConfigHash(config: UmbPropertyEditorConfigCollection | undefined) {
		const configString = config ? JSON.stringify(config.toObject()) : '';
		const hashCode = simpleHashCode(configString);
		this.#configHashCode = hashCode;
	}

	#onChange(event: CustomEvent & { target: UmbInputDocumentElement }) {
		this.value = event.target.value;
		this.dispatchEvent(new UmbChangeEvent());
	}

	#getInteractionMemoryUnique() {
		return `UmbDocumentPickerPropertyEditorUi${this.#configHashCode ? '-' + this.#configHashCode : ''}`;
	}

	#getInteractionMemory() {
		const memoryUnique = this.#getInteractionMemoryUnique();
		if (!memoryUnique) return;
		if (!this.#interactionMemoryContext) return;

		const memory = this.#interactionMemoryContext.memory.getMemory(memoryUnique);
		this._interactionMemories = memory?.memories ?? [];
	}

	#setInteractionMemory(memories: Array<UmbInteractionMemoryModel>) {
		const memoryUnique = this.#getInteractionMemoryUnique();
		if (!memoryUnique) return;
		if (!this.#interactionMemoryContext) return;

		// Set up memory for the Property Editor + Data Type context which includes all memories from the input
		const dataTypeMemory: UmbInteractionMemoryModel = {
			unique: memoryUnique,
			memories,
		};

		this.#interactionMemoryContext.memory.setMemory(dataTypeMemory);
	}

	#deleteInteractionMemory() {
		const unique = this.#getInteractionMemoryUnique();
		if (!unique) return;
		this.#interactionMemoryContext?.memory.deleteMemory(unique);
	}

	#onInteractionMemoriesChange(event: UmbChangeEvent) {
		const target = event.target as UmbInputDocumentElement;
		const interactionMemories = target.interactionMemories;

		if (interactionMemories && interactionMemories.length > 0) {
			this.#setInteractionMemory(interactionMemories);
		} else {
			this.#deleteInteractionMemory();
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
				.interactionMemories=${this._interactionMemories}
				@interaction-memory-change=${this.#onInteractionMemoriesChange}>
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
