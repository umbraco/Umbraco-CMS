import { localizeOperators, localizePropertyType } from './utils.js';
import type { UUIComboboxListElement } from '@umbraco-cms/backoffice/external/uui';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { css, html, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type {
	OperatorModel,
	TemplateQueryExecuteFilterPresentationModel,
	TemplateQuerySettingsResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { TemplateQueryPropertyTypeModel } from '@umbraco-cms/backoffice/external/backend-api';

@customElement('umb-template-query-builder-filter')
export class UmbTemplateQueryBuilderFilterElement extends UmbLitElement {
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
	};

	#setConstrainValue = (e: Event) => {
		const target = e.target as UUIComboboxListElement;
		this.filter = { ...this.filter, constraintValue: target.value as string };
	};

	#setOperator = (e: Event) => {
		const target = e.target as UUIComboboxListElement;
		this.filter = { ...this.filter, operator: target.value as OperatorModel };
	};

	#resetOperator() {
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this.filter = { ...this.filter, operator: undefined, constraintValue: undefined };
	}

	#resetFilter() {
		this.filter = <TemplateQueryExecuteFilterPresentationModel>{};
		this.dispatchEvent(new Event('remove-filter'));
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

	protected override willUpdate(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		if (_changedProperties.has('filter')) {
			if (this.isFilterValid) {
				this.dispatchEvent(new Event('update-query'));
			}
		}
	}

	private _renderOperatorsDropdown() {
		if (!this.settings?.operators) return;
		const operators = localizeOperators(this.settings?.operators, this.currentPropertyType);

		return html`<umb-dropdown look="outline" id="operator-dropdown" label="Choose operator">
			<span slot="label">${this.filter?.operator ?? ''}</span>
			<uui-combobox-list @change=${this.#setOperator} class="options-list">
				${operators
					?.filter((operator) =>
						this.currentPropertyType ? operator.applicableTypes?.includes(this.currentPropertyType) : true,
					)
					.map(
						(operator) =>
							html`<uui-combobox-list-option .value=${(operator.operator as string) ?? ''}>
								<umb-localize .key=${operator.localizeKey!}> ${operator.operator} </umb-localize>
							</uui-combobox-list-option>`,
					)}
			</uui-combobox-list>
		</umb-dropdown>`;
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

	override render() {
		const properties = localizePropertyType(this.settings?.properties);
		return html`
			<span>${this.unremovable ? this.localize.term('template_where') : this.localize.term('template_and')}</span>
			<umb-dropdown look="outline" id="property-alias-dropdown" label="Property alias">
				<span slot="label">${this.filter?.propertyAlias ?? ''}</span>
				<uui-combobox-list @change=${this.#setPropertyAlias} class="options-list">
					${properties?.map(
						(property) =>
							html`<uui-combobox-list-option tabindex="0" .value=${property.alias ?? ''}>
								<umb-localize key=${ifDefined(property.localizeKey)}> ${property.alias}</umb-localize>
							</uui-combobox-list-option>`,
					)}
				</uui-combobox-list></umb-dropdown
			>
			${this.filter?.propertyAlias ? this._renderOperatorsDropdown() : ''}
			${this.filter?.operator ? this._renderConstraintValueInput() : ''}
			<uui-button-group>
				<uui-button
					title=${this.localize.term('general_add')}
					label=${this.localize.term('general_add')}
					compact
					@click=${this.#addFilter}>
					<uui-icon name="icon-add"></uui-icon>
				</uui-button>
				<uui-button
					title=${this.localize.term('general_remove')}
					label=${this.localize.term('general_remove')}
					compact
					@click=${this.#removeOrReset}>
					<uui-icon name="delete"></uui-icon>
				</uui-button>
			</uui-button-group>
		`;
	}

	static override styles = [
		css`
			:host {
				display: flex;
				gap: 10px;
				border-bottom: 1px solid #f3f3f5;
				align-items: center;
				padding: 20px 0;
			}

			uui-combobox-list-option {
				padding: 8px 20px;
				margin: 0;
			}
		`,
	];
}

export default UmbTemplateQueryBuilderFilterElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-template-query-builder-filter': UmbTemplateQueryBuilderFilterElement;
	}
}
