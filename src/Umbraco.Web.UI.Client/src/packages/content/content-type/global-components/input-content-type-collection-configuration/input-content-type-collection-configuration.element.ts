import { html, customElement, property, css, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import {
	UMB_DATATYPE_WORKSPACE_MODAL,
	UMB_DATA_TYPE_ENTITY_TYPE,
	UMB_DATA_TYPE_PICKER_FLOW_DATA_TYPE_PICKER_MODAL,
} from '@umbraco-cms/backoffice/data-type';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';

@customElement('umb-input-content-type-collection-configuration')
export class UmbInputContentTypeCollectionConfigurationElement extends UmbFormControlMixin<
	string,
	typeof UmbLitElement
>(UmbLitElement) {
	protected override getFormElement() {
		return undefined;
	}

	#dataTypeModal;

	#propertyEditorUiAlias = 'Umb.PropertyEditorUi.Collection';

	@state()
	private _dataTypePickerModalPath?: string;

	@property({ attribute: 'default-value' })
	defaultValue?: string;

	#setValue(value: string | undefined) {
		this.value = value;
		this.dispatchEvent(new UmbChangeEvent());
	}

	constructor() {
		super();

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

		this.#dataTypeModal = new UmbModalRouteRegistrationController(this, UMB_DATATYPE_WORKSPACE_MODAL)
			.addAdditionalPath(':uiAlias')
			.onSetup((params) => {
				return { data: { entityType: UMB_DATA_TYPE_ENTITY_TYPE, preset: { editorUiAlias: params.uiAlias } } };
			})
			.onSubmit((value) => {
				this.#setValue(value?.unique ?? this.defaultValue ?? '');
			});
	}

	#clearDataType() {
		this.#setValue(undefined);
	}

	#createDataType() {
		this.#dataTypeModal.open(
			{ uiAlias: this.#propertyEditorUiAlias },
			`create/parent/${UMB_DATA_TYPE_ENTITY_TYPE}/null`,
		);
	}

	#editDataType() {
		this.#dataTypeModal?.open({}, `edit/${this.value}`);
	}

	override render() {
		return !this.value ? this.#renderCreate() : this.#renderConfigured();
	}

	#renderCreate() {
		if (!this._dataTypePickerModalPath) return nothing;
		return html`
			<uui-button
				id="create-button"
				color="default"
				look="placeholder"
				label=${this.localize.term('collection_addCollectionConfiguration')}
				href=${this._dataTypePickerModalPath}></uui-button>
		`;
	}

	#renderConfigured() {
		if (!this.value || !this._dataTypePickerModalPath) return nothing;
		return html`
			<uui-ref-list>
				<umb-ref-data-type standalone data-type-id=${this.value} @open=${this.#editDataType}>
					<uui-action-bar slot="actions">
						<uui-button
							label=${this.localize.term('general_choose')}
							href=${this._dataTypePickerModalPath}></uui-button>
						<uui-button @click=${this.#clearDataType} label=${this.localize.term('general_remove')}></uui-button>
					</uui-action-bar>
				</umb-ref-data-type>
			</uui-ref-list>
		`;
	}

	static override styles = [
		css`
			#create-button {
				width: 100%;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-content-type-collection-configuration': UmbInputContentTypeCollectionConfigurationElement;
	}
}
