import type { UmbInputTreeElement } from './components/input-tree/index.js';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbDynamicRootRepository } from '@umbraco-cms/backoffice/dynamic-root';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbTreePickerSource } from '@umbraco-cms/backoffice/components';

// import of local component
import './components/input-tree/index.js';

/**
 * @element umb-property-editor-ui-tree-picker
 */
@customElement('umb-property-editor-ui-tree-picker')
export class UmbPropertyEditorUITreePickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property({ type: Array })
	value: UmbInputTreeElement['items'] = [];

	@state()
	type: UmbTreePickerSource['type'] = 'content';

	@state()
	startNodeId?: string | null;

	@state()
	min = 0;

	@state()
	max = Infinity;

	@state()
	allowedContentTypeIds?: string | null;

	@state()
	showOpenButton?: boolean;

	@state()
	ignoreUserStartNodes?: boolean;

	#dynamicRoot?: UmbTreePickerSource['dynamicRoot'];

	#dynamicRootRepository = new UmbDynamicRootRepository(this);

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		const startNode = config.getValueByAlias<UmbTreePickerSource>('startNode');
		if (startNode) {
			this.type = startNode.type;
			this.startNodeId = startNode.id;
			this.#dynamicRoot = startNode.dynamicRoot;
		}

		this.min = Number(config.getValueByAlias('minNumber')) || 0;
		this.max = Number(config.getValueByAlias('maxNumber')) || Infinity;

		this.allowedContentTypeIds = config.getValueByAlias('filter');
		this.showOpenButton = config.getValueByAlias('showOpenButton');
		this.ignoreUserStartNodes = config.getValueByAlias('ignoreUserStartNodes');
	}

	connectedCallback() {
		super.connectedCallback();

		this.#setStartNodeId();
	}

	async #setStartNodeId() {
		if (this.startNodeId) return;

		// TODO: Awaiting the workspace context to have a parent entity ID value. [LK]
		// e.g. const parentEntityId = this.#workspaceContext?.getParentEntityId();
		const workspaceContext = await this.getContext(UMB_ENTITY_WORKSPACE_CONTEXT);
		const unique = workspaceContext.getUnique();
		if (unique && this.#dynamicRoot) {
			const result = await this.#dynamicRootRepository.postDynamicRootQuery(this.#dynamicRoot, unique);
			if (result && result.length > 0) {
				this.startNodeId = result[0];
			}
		}
	}

	#onChange(event: CustomEvent & { target: UmbInputTreeElement }) {
		this.value = event.target.items;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`<umb-input-tree
			.items=${this.value}
			.type=${this.type}
			.startNodeId=${this.startNodeId ?? ''}
			.min=${this.min}
			.max=${this.max}
			.allowedContentTypeIds=${this.allowedContentTypeIds ?? ''}
			?showOpenButton=${this.showOpenButton}
			?ignoreUserStartNodes=${this.ignoreUserStartNodes}
			@change=${this.#onChange}></umb-input-tree>`;
	}
}

export default UmbPropertyEditorUITreePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tree-picker': UmbPropertyEditorUITreePickerElement;
	}
}
