import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import type { UmbDataTypeModel } from '../../models.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbModalRouteRegistrationController, UMB_DATA_TYPE_PICKER_FLOW_MODAL } from '@umbraco-cms/backoffice/modal';
import { UmbRepositoryItemsManager } from '@umbraco-cms/backoffice/repository';

// Note: Does only support picking a single data type. But this could be developed later into this same component. To follow other picker input components.
/**
 * Form control for picking or creating a data type.
 * @element umb-data-type-flow-input
 * @fires change - when the value of the input changes
 * @fires blur - when the input loses focus
 * @fires focus - when the input gains focus
 */
@customElement('umb-data-type-flow-input')
export class UmbInputDataTypeElement extends FormControlMixin(UmbLitElement) {
	#itemsManager;

	protected getFormElement() {
		return undefined;
	}

	@state()
	private _items?: Array<UmbDataTypeModel>;

	/**
	 * @param {string} dataTypeId
	 * @default []
	 */
	@property({ attribute: false })
	get value(): string {
		return super.value?.toString() ?? '';
	}
	set value(dataTypeId: string) {
		super.value = dataTypeId ?? '';
		this.#itemsManager.setUniques(super.value.split(','));
	}

	@state()
	private _modalRoute?: string;

	constructor() {
		super();

		this.#itemsManager = new UmbRepositoryItemsManager<UmbDataTypeModel>(this, 'Umb.Repository.DataType');
		this.observe(this.#itemsManager.uniques, (uniques) => {
			super.value = uniques.join(',');
		});
		this.observe(this.#itemsManager.items, (items) => {
			this._items = items;
		});

		new UmbModalRouteRegistrationController(this, UMB_DATA_TYPE_PICKER_FLOW_MODAL)
			.onSetup(() => {
				return {
					selection: this.#itemsManager.getUniques(),
					submitLabel: 'Submit',
				};
			})
			.onSubmit((submitData) => {
				// TODO: we might should set the alias to null or empty string, if no selection.
				this.#itemsManager.setUniques(submitData.selection);
				this.dispatchEvent(new CustomEvent('change', { composed: true, bubbles: true }));
			})
			.observeRouteBuilder((routeBuilder) => {
				this._modalRoute = routeBuilder(null);
				this.requestUpdate('_modalRoute');
			});
	}

	render() {
		return this._items && this._items.length > 0
			? html`
					<umb-ref-data-type
						name=${this._items[0].name}
						property-editor-ui-alias=${this._items[0].propertyEditorAlias}
						property-editor-model-alias=${this._items[0].propertyEditorUiAlias}
						@open=${() => {
							console.warn('TO BE DONE..');
						}}
						border>
						<!-- TODO: Get the icon from property editor UI -->
						<uui-icon name="${'document'}" slot="icon"></uui-icon>
						<uui-action-bar slot="actions">
							<uui-button label="Change" .href=${this._modalRoute}></uui-button>
						</uui-action-bar>
					</umb-ref-data-type>
			  `
			: html`
					<uui-button
						id="empty-state-button"
						label="Select Property Editor"
						look="placeholder"
						color="default"
						.href=${this._modalRoute}></uui-button>
			  `;
	}

	static styles = [
		UUITextStyles,
		css`
			#empty-state-button {
				width: 100%;
				--uui-button-padding-top-factor: 4;
				--uui-button-padding-bottom-factor: 4;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-data-type-flow-input': UmbInputDataTypeElement;
	}
}
