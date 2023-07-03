import { UUITextStyles, type UUIComboboxListElement } from '@umbraco-cms/backoffice/external/uui';
import { PropertyValueMap, css, html, customElement, property, query, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import {
	OperatorModel,
	TemplateQueryExecuteFilterPresentationModel,
	TemplateQueryPropertyTypeModel,
	TemplateQuerySettingsResponseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbButtonWithDropdownElement } from '@umbraco-cms/backoffice/components';

@customElement('umb-query-builder-filter')
export class UmbQueryBuilderFilterElement extends UmbLitElement {
	@query('#property-alias-dropdown')
	private _propertyAliasDropdown?: UmbButtonWithDropdownElement;

	@query('#operator-dropdown')
	private _operatorDropdown?: UmbButtonWithDropdownElement;

	@property({ type: Object, attribute: false })
	filter: TemplateQueryExecuteFilterPresentationModel = <TemplateQueryExecuteFilterPresentationModel>{};

	@property({ type: Boolean })
	unremovable = false;

	@property({ type: Object, attribute: false })
	settings?: TemplateQuerySettingsResponseModel;

	@state()
	currentPropertyType: TemplateQueryPropertyTypeModel | null = null;

	#setPropertyAlias = (e: Event) => {
		const target = e.target as UUIComboboxListElement;
		const oldCurrentPropertyType = this.currentPropertyType;
		this.filter = { ...this.filter, propertyAlias: target.value as string };
		this.currentPropertyType =
			this.settings?.properties?.find((p) => p.alias === this.filter.propertyAlias)?.type ?? null;
		if (oldCurrentPropertyType !== this.currentPropertyType) this.#resetOperator();
		this._propertyAliasDropdown?.closePopover();
	};

	#setConstrainValue = (e: Event) => {
		const target = e.target as UUIComboboxListElement;
		this.filter = { ...this.filter, constraintValue: target.value as string };
	};

	#setOperator = (e: Event) => {
		const target = e.target as UUIComboboxListElement;
		this.filter = { ...this.filter, operator: target.value as OperatorModel };
		this._operatorDropdown?.closePopover();
	};

	#resetOperator() {
		this.filter = { ...this.filter, operator: undefined };
	}

	#resetFilter() {
		this.filter = <TemplateQueryExecuteFilterPresentationModel>{};
	}

	#removeOrReset() {
		if (this.unremovable) this.#resetFilter();
		else this.dispatchEvent(new Event('remove-filter'));
	}

	#addFilter() {
		this.dispatchEvent(new Event('add-filter'));
	}

    get isFilterValid(): boolean {
        return Object.keys(this.filter).length === 3 && Object.values(this.filter).every((v) => !!v);
    }

    protected willUpdate(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
        if (_changedProperties.has('filter')) {

            if (this.isFilterValid) {
                this.dispatchEvent(new Event('update-query'));
            }
        }
    }

	private _renderOperatorsDropdown() {
		return html`<umb-button-with-dropdown look="outline" id="operator-dropdown" label="choose operator">
			${this.filter?.operator ?? ''}
			<uui-combobox-list slot="dropdown" @change=${this.#setOperator} class="options-list">
				${this.settings?.operators
					?.filter((operator) =>
						this.currentPropertyType ? operator.applicableTypes?.includes(this.currentPropertyType) : true
					)
					.map(
						(operator) =>
							html`<uui-combobox-list-option .value=${(operator.operator as string) ?? ''}
								>${operator.operator}</uui-combobox-list-option
							>`
					)}</uui-combobox-list
			>
		</umb-button-with-dropdown>`;
	}

	private _renderConstraintValueInput() {
		switch (this.currentPropertyType) {
			case TemplateQueryPropertyTypeModel.INTEGER:
				return html`<uui-input type="number" @change=${this.#setConstrainValue} label="constrain value"></uui-input>`;
			case TemplateQueryPropertyTypeModel.STRING:
				return html`<uui-input type="text" @change=${this.#setConstrainValue} label="constrain value"></uui-input>`;
			case TemplateQueryPropertyTypeModel.DATE_TIME:
				return html`<uui-input type="date" @change=${this.#setConstrainValue} label="constrain value"></uui-input>`;
			default:
				return html`<uui-input type="text" @change=${this.#setConstrainValue} label="constrain value"></uui-input>`;
		}
	}

	render() {
		return html`
			<span>${this.unremovable ? 'where' : 'and'}</span>
			<umb-button-with-dropdown look="outline" id="property-alias-dropdown" label="Property alias"
				>${this.filter?.propertyAlias ?? ''}
				<uui-combobox-list  slot="dropdown" @change=${this.#setPropertyAlias} class="options-list">
					${this.settings?.properties?.map(
						(property) =>
							html`<uui-combobox-list-option tabindex="0" .value=${property.alias ?? ''}
								>${property.alias}</uui-combobox-list-option
							>`
					)}
				</uui-combobox-list></umb-button-with-dropdown
			>
			${this.filter?.propertyAlias ? this._renderOperatorsDropdown() : ''}
			${this.filter?.operator ? this._renderConstraintValueInput() : ''}
			<uui-button-group>
				<uui-button title="Add filter" label="Add filter" compact @click=${this.#addFilter}
					><uui-icon name="add"></uui-icon
				></uui-button>
				<uui-button title="Remove filter" label="Remove filter" compact @click=${this.#removeOrReset}
					><uui-icon name="delete"></uui-icon
				></uui-button>
			</uui-button-group>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				gap: 10px;
				border-bottom: 1px solid #f3f3f5;
				align-items: center;
				padding: 20px 0;
			}

			.options-list {
				min-width: 25ch;
				background-color: var(--uui-color-surface);
				box-shadow: var(--uui-shadow-depth-3);
			}

			uui-combobox-list-option {
				padding: 8px 20px;
			}
		`,
	];
}

export default UmbQueryBuilderFilterElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-query-builder-filter': UmbQueryBuilderFilterElement;
	}
}
