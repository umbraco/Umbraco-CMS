import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, property, query, state } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import {
	OperatorModel,
	TemplateQueryExecuteFilterPresentationModel,
	TemplateQueryPropertyTypeModel,
	TemplateQuerySettingsResponseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UUIComboboxListElement } from '@umbraco-ui/uui';
import { UmbButtonWithDropdownElement } from '@umbraco-cms/backoffice/components';

@customElement('umb-query-builder-filter')
export class UmbQueryBuilderFilterElement extends UmbLitElement {
	@query('#property-alias-dropdown')
	private _propertyAliasDropdown?: UmbButtonWithDropdownElement;

	@query('#operator-dropdown')
	private _operatorDropdown?: UmbButtonWithDropdownElement;

	@property({ type: Object, attribute: false })
	filter: TemplateQueryExecuteFilterPresentationModel = <TemplateQueryExecuteFilterPresentationModel>{};

	@property({ type: Object, attribute: false })
	settings: TemplateQuerySettingsResponseModel | null = null;

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

	private _renderOperatorsDropdown() {
		return html`<umb-button-with-dropdown look="outline" id="operator-dropdown">
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
				return html`<uui-input type="number" @change=${this.#setConstrainValue}></uui-input>`;
			case TemplateQueryPropertyTypeModel.STRING:
				return html`<uui-input type="text" @change=${this.#setConstrainValue}></uui-input>`;
			case TemplateQueryPropertyTypeModel.DATE_TIME:
				return html`<uui-input type="datetime-local" @change=${this.#setConstrainValue}></uui-input>`;
			default:
				return html`<input type="text" @change=${this.#setConstrainValue} />`;
		}
	}

	render() {
		return html`where
			<umb-button-with-dropdown look="outline" id="property-alias-dropdown"
				>${this.filter?.propertyAlias ?? ''}
				<uui-combobox-list slot="dropdown" @change=${this.#setPropertyAlias} class="options-list">
					${this.settings?.properties?.map(
						(property) =>
							html`<uui-combobox-list-option .value=${property.alias ?? ''}
								>${property.alias}</uui-combobox-list-option
							>`
					)}
				</uui-combobox-list></umb-button-with-dropdown
			>
			${this.filter?.propertyAlias ? this._renderOperatorsDropdown() : ''}
			${this.filter?.operator ? this._renderConstraintValueInput() : ''} `;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				align-items: center;
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
