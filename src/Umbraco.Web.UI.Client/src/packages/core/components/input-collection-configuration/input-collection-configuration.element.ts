import { html, customElement, property, css, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbRepositoryItemsManager } from '@umbraco-cms/backoffice/repository';
import { UMB_DATATYPE_WORKSPACE_MODAL, UMB_DATA_TYPE_ITEM_REPOSITORY_ALIAS } from '@umbraco-cms/backoffice/data-type';
import {
	UmbModalRouteRegistrationController,
	UMB_DATA_TYPE_PICKER_FLOW_DATA_TYPE_PICKER_MODAL,
} from '@umbraco-cms/backoffice/modal';
import type { UmbDataTypeItemModel } from '@umbraco-cms/backoffice/data-type';

@customElement('umb-input-collection-configuration')
export class UmbInputCollectionConfigurationElement extends FormControlMixin(UmbLitElement) {
	protected getFormElement() {
		return undefined;
	}

	#itemManager = new UmbRepositoryItemsManager<UmbDataTypeItemModel>(
		this,
		UMB_DATA_TYPE_ITEM_REPOSITORY_ALIAS,
		(x) => x.unique,
	);

	#createDataTypeModal: UmbModalRouteRegistrationController;

	#propertyEditorUiAlias = 'Umb.PropertyEditorUi.CollectionView';

	@state()
	private _dataTypePickerModalPath?: string;

	@state()
	private _item?: UmbDataTypeItemModel;

	@property({ attribute: 'default-value' })
	defaultValue?: string;

	#setValue(value: string) {
		this.value = value;
		this.#itemManager.setUniques(value ? [value] : []);
		this.dispatchEvent(new UmbChangeEvent());
	}

	constructor() {
		super();

		this.observe(this.#itemManager.items, (items) => {
			this._item = items[0];
		});

		new UmbModalRouteRegistrationController(this, UMB_DATA_TYPE_PICKER_FLOW_DATA_TYPE_PICKER_MODAL)
			.addAdditionalPath(':uiAlias')
			.onSetup((routingInfo) => {
				return {
					data: {
						propertyEditorUiAlias: routingInfo.uiAlias,
					},
					value: undefined,
				};
			})
			.onSubmit((submitData) => {
				if (submitData?.createNewWithPropertyEditorUiAlias) {
					this.#createDataType();
				} else {
					this.#setValue(submitData?.dataTypeId ?? this.defaultValue ?? '');
				}
			})
			.observeRouteBuilder((routeBuilder) => {
				this._dataTypePickerModalPath = routeBuilder({ uiAlias: this.#propertyEditorUiAlias });
			});

		this.#createDataTypeModal = new UmbModalRouteRegistrationController(this, UMB_DATATYPE_WORKSPACE_MODAL)
			.addAdditionalPath(':uiAlias')
			.onSetup((params) => {
				return { data: { entityType: 'data-type', preset: { editorUiAlias: params.uiAlias } } };
			})
			.onSubmit((value) => {
				this.#setValue(value?.unique ?? this.defaultValue ?? '');
			});
	}

	connectedCallback() {
		super.connectedCallback();

		if (this.value) {
			this.#itemManager.setUniques([this.value as string]);
		}
	}

	#clearDataType() {
		this.#setValue('');
	}

	#createDataType() {
		this.#createDataTypeModal.open({ uiAlias: this.#propertyEditorUiAlias }, 'create/null');
	}

	render() {
		return !this.value ? this.#renderCreate() : this.#renderConfigured();
	}

	#renderCreate() {
		if (!this._dataTypePickerModalPath) return nothing;
		return html`<uui-button
			id="create-button"
			color="default"
			look="placeholder"
			label="Configure as a collection"
			href=${this._dataTypePickerModalPath}></uui-button>`;
	}

	#renderConfigured() {
		if (!this._item || !this._dataTypePickerModalPath) return nothing;
		return html`
			<uui-ref-list>
				<uui-ref-node-data-type standalone name=${this._item.name} detail=${this._item.unique}>
					<uui-action-bar slot="actions">
						<uui-button
							label=${this.localize.term('general_choose')}
							href=${this._dataTypePickerModalPath}></uui-button>
						<uui-button @click=${this.#clearDataType} label=${this.localize.term('general_remove')}></uui-button>
					</uui-action-bar>
				</uui-ref-node-data-type>
			</uui-ref-list>
		`;
	}

	static styles = [
		css`
			#create-button {
				width: 100%;
			}
		`,
	];
}

export default UmbInputCollectionConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-collection-configuration': UmbInputCollectionConfigurationElement;
	}
}
