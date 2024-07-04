import { UmbContentPickerDynamicRootRepository } from './dynamic-root/repository/index.js';
import type { UmbInputContentElement } from './components/input-content/index.js';
import type { UmbContentPickerSource, UmbContentPickerSourceType } from './types.js';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import { UMB_DOCUMENT_ENTITY_TYPE } from '@umbraco-cms/backoffice/document';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UMB_MEDIA_ENTITY_TYPE } from '@umbraco-cms/backoffice/media';
import { UMB_MEMBER_ENTITY_TYPE } from '@umbraco-cms/backoffice/member';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbTreeStartNode } from '@umbraco-cms/backoffice/tree';

// import of local component
import './components/input-content/index.js';

type UmbContentPickerValueType = UmbInputContentElement['selection'];

const elementName = 'umb-property-editor-ui-content-picker';

/**
 * @element umb-property-editor-ui-content-picker
 */
@customElement(elementName)
export class UmbPropertyEditorUIContentPickerElement
	extends UmbFormControlMixin<UmbContentPickerValueType | undefined, typeof UmbLitElement>(UmbLitElement, undefined)
	implements UmbPropertyEditorUiElement
{
	@property({ type: Array })
	public override set value(value: UmbContentPickerValueType | undefined) {
		this.#value = value;
	}
	public override get value(): UmbContentPickerValueType | undefined {
		return this.#value;
	}
	#value?: UmbContentPickerValueType = [];

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
	_rootUnique?: string | null;

	@state()
	_rootEntityType?: string;

	#dynamicRoot?: UmbContentPickerSource['dynamicRoot'];
	#dynamicRootRepository = new UmbContentPickerDynamicRootRepository(this);

	#entityTypeDictionary: { [type in UmbContentPickerSourceType]: string } = {
		content: UMB_DOCUMENT_ENTITY_TYPE,
		media: UMB_MEDIA_ENTITY_TYPE,
		member: UMB_MEMBER_ENTITY_TYPE,
	};

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		const startNode = config.getValueByAlias<UmbContentPickerSource>('startNode');
		if (startNode) {
			this._type = startNode.type;
			this._rootUnique = startNode.id;
			this._rootEntityType = this.#entityTypeDictionary[startNode.type];
			this.#dynamicRoot = startNode.dynamicRoot;
		}

		this._min = Number(config.getValueByAlias('minNumber')) || 0;
		this._max = Number(config.getValueByAlias('maxNumber')) || Infinity;

		this._allowedContentTypeUniques = config.getValueByAlias('filter');
		this._showOpenButton = config.getValueByAlias('showOpenButton');
	}

	override firstUpdated() {
		this.addFormControlElement(this.shadowRoot!.querySelector('umb-input-content')!);
		this.#setPickerRootUnique();
	}

	async #setPickerRootUnique() {
		// If we have a root unique value, we don't need to fetch it from the dynamic root
		if (this._rootUnique) return;
		if (!this.#dynamicRoot) return;

		const workspaceContext = await this.getContext(UMB_ENTITY_WORKSPACE_CONTEXT);
		const unique = workspaceContext.getUnique();
		if (!unique) return;

		const menuStructureWorkspaceContext = (await this.getContext('UmbMenuStructureWorkspaceContext')) as any;
		const parent = (await this.observe(menuStructureWorkspaceContext.parent, () => {})?.asPromise()) as any;
		const parentUnique = parent?.unique;

		const result = await this.#dynamicRootRepository.requestRoot(this.#dynamicRoot, unique, parentUnique);
		if (result && result.length > 0) {
			this._rootUnique = result[0];
		}
	}

	#onChange(event: CustomEvent & { target: UmbInputContentElement }) {
		this.value = event.target.selection;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	override render() {
		const startNode: UmbTreeStartNode | undefined =
			this._rootUnique && this._rootEntityType
				? { unique: this._rootUnique, entityType: this._rootEntityType }
				: undefined;

		return html`
			<umb-input-content
				.selection=${this.value ?? []}
				.type=${this._type}
				.min=${this._min}
				.minMessage=${this._minMessage}
				.max=${this._max}
				.maxMessage=${this._maxMessage}
				.startNode=${startNode}
				.allowedContentTypeIds=${this._allowedContentTypeUniques ?? ''}
				?showOpenButton=${this._showOpenButton}
				@change=${this.#onChange}></umb-input-content>
		`;
	}
}

export { UmbPropertyEditorUIContentPickerElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbPropertyEditorUIContentPickerElement;
	}
}
