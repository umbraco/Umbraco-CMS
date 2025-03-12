import { UmbContentPickerDynamicRootRepository } from './dynamic-root/repository/index.js';
import type { UmbInputContentElement } from './components/input-content/index.js';
import type { UmbContentPickerSource, UmbContentPickerSourceType } from './types.js';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UMB_DOCUMENT_ENTITY_TYPE } from '@umbraco-cms/backoffice/document';
import { UMB_MEDIA_ENTITY_TYPE } from '@umbraco-cms/backoffice/media';
import { UMB_MEMBER_ENTITY_TYPE } from '@umbraco-cms/backoffice/member';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import type { UmbTreeStartNode } from '@umbraco-cms/backoffice/tree';

// import of local component
import './components/input-content/index.js';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

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

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	@state()
	_type: UmbContentPickerSource['type'] = 'content';

	@state()
	_min = 0;

	@state()
	_minMessage = '';

	@state()
	_max = Infinity;

	@state()
	_maxMessage = '';

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

		this._min = this.#parseInt(config.getValueByAlias('minNumber'), 0);
		this._max = this.#parseInt(config.getValueByAlias('maxNumber'), Infinity);

		this._allowedContentTypeUniques = config.getValueByAlias('filter');
		this._showOpenButton = config.getValueByAlias('showOpenButton');

		this._minMessage = `${this.localize.term('validation_minCount')} ${this._min} ${this.localize.term('validation_items')}`;
		this._maxMessage = `${this.localize.term('validation_maxCount')} ${this._max} ${this.localize.term('validation_itemsSelected')}`;

		// NOTE: Run validation immediately, to notify if the value is outside of min/max range. [LK]
		if (this._min > 0 || this._max < Infinity) {
			this.checkValidity();
		}
	}

	#parseInt(value: unknown, fallback: number): number {
		const num = Number(value);
		return !isNaN(num) && num > 0 ? num : fallback;
	}

	override firstUpdated() {
		this.addFormControlElement(this.shadowRoot!.querySelector('umb-input-content')!);
		this.#setPickerRootUnique();

		if (this._min && this._max && this._min > this._max) {
			console.warn(
				`Property (Content Picker) has been misconfigured, 'minNumber' is greater than 'maxNumber'. Please correct your data type configuration.`,
				this,
			);
		}
	}

	override focus() {
		return this.shadowRoot?.querySelector<UmbInputContentElement>('umb-input-content')?.focus();
	}

	async #setPickerRootUnique() {
		// If we have a root unique value, we don't need to fetch it from the dynamic root
		if (this._rootUnique) return;
		if (!this.#dynamicRoot) return;

		const menuStructureWorkspaceContext = (await this.getContext('UmbMenuStructureWorkspaceContext')) as any;
		const structure = (await this.observe(menuStructureWorkspaceContext.structure, () => {})?.asPromise()) as any[];
		const [parentUnique, unique] = structure?.slice(-2).map((x) => x.unique) ?? [];

		if (!unique) return;

		const result = await this.#dynamicRootRepository.requestRoot(this.#dynamicRoot, unique, parentUnique);
		if (result && result.length > 0) {
			this._rootUnique = result[0];
		}
	}

	#onChange(event: CustomEvent & { target: UmbInputContentElement }) {
		this.value = event.target.selection;
		this.dispatchEvent(new UmbChangeEvent());
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
				?readonly=${this.readonly}
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
