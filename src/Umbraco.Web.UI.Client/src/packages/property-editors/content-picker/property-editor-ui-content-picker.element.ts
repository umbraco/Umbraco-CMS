import { UmbContentPickerDynamicRootRepository } from './dynamic-root/repository/index.js';
import type { UmbInputContentElement } from './components/input-content/index.js';
import type { UmbContentPickerSource, UmbContentPickerSourceType } from './types.js';
import { css, customElement, html, nothing, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UMB_ANCESTORS_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
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

type UmbContentPickerValueType = UmbInputContentElement['selection'];

/**
 * @element umb-property-editor-ui-content-picker
 */
@customElement('umb-property-editor-ui-content-picker')
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
	private _type: UmbContentPickerSource['type'] = 'content';

	@state()
	private _min = 0;

	@state()
	private _minMessage = '';

	@state()
	private _max = Infinity;

	@state()
	private _maxMessage = '';

	@state()
	private _allowedContentTypeUniques?: string | null;

	@state()
	private _rootUnique?: string | null;

	@state()
	private _rootEntityType?: string;

	@state()
	private _invalidData?: UmbContentPickerValueType;

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

			// NOTE: Filter out any items that do not match the entity type. [LK]
			this._invalidData = this.#value?.filter((x) => x.type !== this._rootEntityType);
			if (this._invalidData?.length) {
				this.readonly = true;
			}
		}

		this._min = this.#parseInt(config.getValueByAlias('minNumber'), 0);
		this._max = this.#parseInt(config.getValueByAlias('maxNumber'), Infinity);

		this._allowedContentTypeUniques = config.getValueByAlias('filter');

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

		const ancestorsContext = await this.getContext(UMB_ANCESTORS_ENTITY_CONTEXT);
		const ancestors = ancestorsContext?.getAncestors();
		const [parentUnique, unique] = ancestors?.slice(-2).map((x) => x.unique) ?? [];

		const result = await this.#dynamicRootRepository.requestRoot(this.#dynamicRoot, unique, parentUnique);
		if (result && result.length > 0) {
			this._rootUnique = result[0];
		}
	}

	#onChange(event: CustomEvent & { target: UmbInputContentElement }) {
		this.value = event.target.selection;
		this.dispatchEvent(new UmbChangeEvent());
	}

	async #onRemoveInvalidData() {
		await umbConfirmModal(this, {
			color: 'danger',
			headline: '#contentPicker_unsupportedRemove',
			content: '#defaultdialogs_confirmSure',
			confirmLabel: '#actions_remove',
		});

		this.value = this.value?.filter((x) => x.type === this._rootEntityType);
		this._invalidData = undefined;
		this.readonly = false;
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
				?readonly=${this.readonly}
				@change=${this.#onChange}>
			</umb-input-content>
			${this.#renderInvalidData()}
		`;
	}

	#renderInvalidData() {
		if (!this._invalidData?.length) return nothing;

		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-expect-error
		const groupby = Object.groupBy(this._invalidData, (x) => x.type);
		const grouped = Object.keys(groupby)
			.sort((a, b) => a.localeCompare(b))
			.map((key) => ({ key, items: groupby[key] }));

		const toPickerType = (type: string): UmbContentPickerSourceType => {
			return type === UMB_DOCUMENT_ENTITY_TYPE ? 'content' : (type as UmbContentPickerSourceType);
		};

		return html`
			<div id="messages">
				${repeat(
					grouped,
					(group) => group.key,
					(group) => html`
						<p>
							<umb-localize key="contentPicker_unsupportedHeadline" .args=${[group.key]}>
								<strong>Unsupported ${group.key} items</strong><br />
								The following content is no longer supported in this Editor.
							</umb-localize>
						</p>
						<umb-input-content readonly .selection=${group.items} .type=${toPickerType(group.key)}></umb-input-content>
						<p>
							<umb-localize key="contentPicker_unsupportedMessage">
								If you still require this content, please contact your administrator. Otherwise you can remove it.
							</umb-localize>
						</p>
						<uui-button
							color="danger"
							look="outline"
							label=${this.localize.term('contentPicker_unsupportedRemove')}
							@click=${this.#onRemoveInvalidData}></uui-button>
					`,
				)}
			</div>
		`;
	}

	static override readonly styles = [
		css`
			#messages {
				color: var(--uui-color-danger-standalone);
			}
		`,
	];
}

export { UmbPropertyEditorUIContentPickerElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-content-picker': UmbPropertyEditorUIContentPickerElement;
	}
}
