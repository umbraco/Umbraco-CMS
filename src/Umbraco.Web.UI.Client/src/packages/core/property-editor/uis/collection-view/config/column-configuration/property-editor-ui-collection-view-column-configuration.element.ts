import type { UmbCollectionColumnConfiguration } from '../../../../../collection/types.js';
import { html, customElement, property, repeat, css, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type { UmbInputContentTypePropertyElement } from '@umbraco-cms/backoffice/components';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

/**
 * @element umb-property-editor-ui-collection-view-column-configuration
 */
@customElement('umb-property-editor-ui-collection-view-column-configuration')
export class UmbPropertyEditorUICollectionViewColumnConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	@property({ type: Array })
	value?: Array<UmbCollectionColumnConfiguration> = [];

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	@state()
	private _field?: UmbInputContentTypePropertyElement['selectedProperty'];

	#onChange(e: CustomEvent) {
		const element = e.target as UmbInputContentTypePropertyElement;

		if (!element.selectedProperty) return;

		this._field = element.selectedProperty;

		const duplicate = this.value?.find((config) => this._field?.alias === config.alias);

		if (duplicate) {
			// TODO: Show error to user, can not add duplicate field/column. [LK]
			return;
		}

		const config: UmbCollectionColumnConfiguration = {
			alias: this._field.alias,
			header: this._field.label,
			isSystem: this._field.isSystem ? 1 : 0,
		};

		this.value = [...(this.value ?? []), config];

		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	#onRemove(unique: string) {
		const newValue: Array<UmbCollectionColumnConfiguration> = [];

		this.value?.forEach((config) => {
			if (config.alias !== unique) newValue.push(config);
		});

		this.value = newValue;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	#onHeaderChange(e: UUIInputEvent, configuration: UmbCollectionColumnConfiguration) {
		this.value = this.value?.map(
			(config): UmbCollectionColumnConfiguration =>
				config.alias === configuration.alias ? { ...config, header: e.target.value as string } : config,
		);

		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	#onTemplateChange(e: UUIInputEvent, configuration: UmbCollectionColumnConfiguration) {
		this.value = this.value?.map(
			(config): UmbCollectionColumnConfiguration =>
				config.alias === configuration.alias ? { ...config, nameTemplate: e.target.value as string } : config,
		);

		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`
			${this.#renderTable()}

			<umb-input-content-type-property
				document-types
				media-types
				@change=${this.#onChange}></umb-input-content-type-property>
		`;
	}

	#renderTable() {
		if (!this.value?.length) return;
		return html`
			<uui-table>
				<uui-table-row>
					<uui-table-head-cell style="width:0"></uui-table-head-cell>
					<uui-table-head-cell>Alias</uui-table-head-cell>
					<uui-table-head-cell>Header</uui-table-head-cell>
					<uui-table-head-cell>Template</uui-table-head-cell>
					<uui-table-head-cell style="width:0"></uui-table-head-cell>
				</uui-table-row>
				${repeat(
					this.value,
					(configuration) => configuration.alias,
					(configuration) => html`
						<uui-table-row>
							<uui-table-cell><uui-icon name="icon-navigation"></uui-icon></uui-table-cell>
							${configuration.isSystem === 1
								? this.#renderSystemFieldRow(configuration)
								: this.#renderCustomFieldRow(configuration)}
							<uui-table-cell>
								<uui-button
									label=${this.localize.term('general_remove')}
									look="secondary"
									@click=${() => this.#onRemove(configuration.alias)}></uui-button>
							</uui-table-cell>
						</uui-table-row>
					`,
				)}
			</uui-table>
		`;
	}

	#renderSystemFieldRow(configuration: UmbCollectionColumnConfiguration) {
		return html`
			<uui-table-cell><strong>${configuration.alias}</strong> <small>(system field)</small></uui-table-cell>
			<uui-table-cell>${configuration.header}</uui-table-cell>
			<uui-table-cell></uui-table-cell>
		`;
	}

	#renderCustomFieldRow(configuration: UmbCollectionColumnConfiguration) {
		return html`
			<uui-table-cell><strong>${configuration.alias}</strong></uui-table-cell>
			<uui-table-cell>
				<uui-input
					label="header"
					.value=${configuration.header ?? ''}
					required
					@change=${(e: UUIInputEvent) => this.#onHeaderChange(e, configuration)}></uui-input>
			</uui-table-cell>
			<uui-table-cell>
				<uui-input
					label="template"
					.value=${configuration.nameTemplate ?? ''}
					@change=${(e: UUIInputEvent) => this.#onTemplateChange(e, configuration)}></uui-input>
			</uui-table-cell>
		`;
	}

	static styles = [
		css`
			:host {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-1);
			}

			uui-input {
				width: 100%;
			}
		`,
	];
}

export default UmbPropertyEditorUICollectionViewColumnConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-collection-view-column-configuration': UmbPropertyEditorUICollectionViewColumnConfigurationElement;
	}
}
