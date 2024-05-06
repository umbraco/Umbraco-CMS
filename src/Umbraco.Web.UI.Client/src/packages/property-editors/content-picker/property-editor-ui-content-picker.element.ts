import { UmbContentPickerDynamicRootRepository } from './dynamic-root/repository/index.js';
import type { UmbInputContentElement } from './components/input-content/index.js';
import type { UmbContentPickerSource } from './types.js';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';

// import of local component
import './components/input-content/index.js';

/**
 * @element umb-property-editor-ui-content-picker
 */
@customElement('umb-property-editor-ui-content-picker')
export class UmbPropertyEditorUIContentPickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property({ type: Array })
	value: UmbInputContentElement['items'] = [];

	@state()
	_type: UmbContentPickerSource['type'] = 'content';

	@state()
	_min = 0;

	@state()
	_max = Infinity;

	@state()
	_allowedContentTypeUniques?: string | null;

	@state()
	_showOpenButton?: boolean;

	@state()
	_ignoreUserStartNodes?: boolean;

	@state()
	_rootUnique?: string | null;

	#dynamicRoot?: UmbContentPickerSource['dynamicRoot'];
	#dynamicRootRepository = new UmbContentPickerDynamicRootRepository(this);

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		const startNode = config.getValueByAlias<UmbContentPickerSource>('startNode');
		if (startNode) {
			this._type = startNode.type;
			this._rootUnique = startNode.id;
			this.#dynamicRoot = startNode.dynamicRoot;
		}

		this._min = Number(config.getValueByAlias('minNumber')) || 0;
		this._max = Number(config.getValueByAlias('maxNumber')) || Infinity;

		this._allowedContentTypeUniques = config.getValueByAlias('filter');
		this._showOpenButton = config.getValueByAlias('showOpenButton');
		this._ignoreUserStartNodes = config.getValueByAlias('ignoreUserStartNodes');
	}

	connectedCallback() {
		super.connectedCallback();
		this.#setPickerRootUnique();
	}

	async #setPickerRootUnique() {
		// If we have a root unique value, we don't need to fetch it from the dynamic root
		if (this._rootUnique) return;

		// TODO: Awaiting the workspace context to have a parent entity ID value. [LK]
		// e.g. const parentEntityId = this.#workspaceContext?.getParentEntityId();
		const workspaceContext = await this.getContext(UMB_ENTITY_WORKSPACE_CONTEXT);
		const unique = workspaceContext.getUnique();
		if (unique && this.#dynamicRoot) {
			const result = await this.#dynamicRootRepository.requestRoot(this.#dynamicRoot, unique);
			if (result && result.length > 0) {
				this._rootUnique = result[0];
			}
		}
	}

	#onChange(event: CustomEvent & { target: UmbInputContentElement }) {
		this.value = event.target.items;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		const startFrom = this._rootUnique ? { unique: this._rootUnique } : undefined;

		return html`<umb-input-content
			.items=${this.value}
			.type=${this._type}
			.min=${this._min}
			.max=${this._max}
			.startFrom=${startFrom}
			.allowedContentTypeIds=${this._allowedContentTypeUniques ?? ''}
			?showOpenButton=${this._showOpenButton}
			?ignoreUserStartNodes=${this._ignoreUserStartNodes}
			@change=${this.#onChange}></umb-input-content>`;
	}
}

export default UmbPropertyEditorUIContentPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-content-picker': UmbPropertyEditorUIContentPickerElement;
	}
}
