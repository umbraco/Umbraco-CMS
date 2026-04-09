import { UMB_ELEMENT_FOLDER_ENTITY_TYPE } from '../../entity.js';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbFormControlMixin, UMB_VALIDATION_EMPTY_LOCALIZATION_KEY } from '@umbraco-cms/backoffice/validation';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbNumberRangeValueType } from '@umbraco-cms/backoffice/models';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/property-editor';
import type { UmbTreeStartNode } from '@umbraco-cms/backoffice/tree';

@customElement('umb-element-picker-property-editor-ui')
export class UmbElementPickerPropertyEditorUIElement
	extends UmbFormControlMixin<Array<string> | undefined, typeof UmbLitElement>(UmbLitElement, undefined)
	implements UmbPropertyEditorUiElement
{
	@property({ type: Boolean })
	mandatory?: boolean;

	@property({ type: String })
	mandatoryMessage = UMB_VALIDATION_EMPTY_LOCALIZATION_KEY;

	@property({ type: String })
	name?: string;

	@property({ type: Boolean, reflect: true })
	readonly = false;

	public set config(config: UmbPropertyEditorUiElement['config'] | undefined) {
		if (!config) return;

		this._folderOnly = Boolean(config.getValueByAlias('folderOnly'));

		const minMax = config?.getValueByAlias<UmbNumberRangeValueType>('validationLimit');
		this._min = minMax?.min ?? 0;
		this._max = minMax?.max ?? Infinity;

		this._minMessage = `${this.localize.term('validation_minCount')} ${this._min} ${this.localize.term('validation_items')}`;
		this._maxMessage = `${this.localize.term('validation_maxCount')} ${this._max} ${this.localize.term('validation_itemsSelected')}`;

		const startNodeId = config.getValueByAlias<Array<string>>('startNodeId') ?? [];
		this._startNode = startNodeId.length
			? { unique: startNodeId[0], entityType: UMB_ELEMENT_FOLDER_ENTITY_TYPE }
			: undefined;
	}

	@state()
	private _folderOnly = false;

	@state()
	private _min = 0;

	@state()
	private _minMessage = '';

	@state()
	private _max = Infinity;

	@state()
	private _maxMessage = '';

	@state()
	private _startNode?: UmbTreeStartNode;

	override focus() {
		return this.shadowRoot?.querySelector('umb-input-element')?.focus();
	}

	override firstUpdated(changedProperties: Map<string | number | symbol, unknown>) {
		super.firstUpdated(changedProperties);

		this.addFormControlElement(this.shadowRoot!.querySelector('umb-input-element')!);

		if (this._min && this._max && this._min > this._max) {
			console.warn(
				`Property (Element Picker) has been misconfigured, 'min' is greater than 'max'. Please correct your data type configuration.`,
				this,
			);
		}
	}

	#onChange(event: CustomEvent & { target: { selection: Array<string> } }) {
		this.value = event.target.selection;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<umb-input-element
				.selection=${this.value ?? []}
				.startNode=${this._startNode}
				.min=${this._min}
				.minMessage=${this._minMessage}
				.max=${this._max}
				.maxMessage=${this._maxMessage}
				?folderOnly=${this._folderOnly}
				?readonly=${this.readonly}
				@change=${this.#onChange}>
			</umb-input-element>
		`;
	}
}

export { UmbElementPickerPropertyEditorUIElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-element-picker-property-editor-ui': UmbElementPickerPropertyEditorUIElement;
	}
}
