import type { UmbInputCollectionContentTypePropertyElement } from './components/index.js';
import type { UmbCollectionColumnConfiguration } from '@umbraco-cms/backoffice/collection';
import { css, customElement, html, nothing, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

// import of local components
import './components/index.js';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

/**
 * @element umb-property-editor-ui-collection-column-configuration
 */
@customElement('umb-property-editor-ui-collection-column-configuration')
export class UmbPropertyEditorUICollectionColumnConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	#sorter = new UmbSorterController<UmbCollectionColumnConfiguration>(this, {
		getUniqueOfElement: (element) => element.id,
		getUniqueOfModel: (modelEntry) => modelEntry.alias,
		itemSelector: '.layout-item',
		containerSelector: '#layout-wrapper',
		onChange: ({ model }) => {
			this.value = model;
			this.dispatchEvent(new UmbChangeEvent());
		},
	});

	@property({ type: Array })
	public set value(value: Array<UmbCollectionColumnConfiguration> | undefined) {
		this.#value = value ?? [];
		this.#sorter.setModel(this.#value);
	}
	public get value(): Array<UmbCollectionColumnConfiguration> {
		return this.#value;
	}
	#value: Array<UmbCollectionColumnConfiguration> = [];

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	@state()
	private _field?: UmbInputCollectionContentTypePropertyElement['selectedProperty'];

	#onAdd(e: CustomEvent) {
		const element = e.target as UmbInputCollectionContentTypePropertyElement;

		if (!element.selectedProperty) return;

		this._field = element.selectedProperty;

		const duplicate = this.value?.find((config) => this._field?.alias === config.alias);

		if (duplicate) {
			// TODO: Show error to user, can not add duplicate field/column. [LK]
			throw new Error('Duplicate field/columns are not allowed.');
			return;
		}

		const config: UmbCollectionColumnConfiguration = {
			alias: this._field.alias,
			header: this._field.label,
			isSystem: this._field.isSystem ? 1 : 0,
		};

		this.value = [...(this.value ?? []), config];

		this.dispatchEvent(new UmbChangeEvent());
	}

	#onChangeLabel(e: UUIInputEvent, configuration: UmbCollectionColumnConfiguration) {
		this.value = this.value?.map(
			(config): UmbCollectionColumnConfiguration =>
				config.alias === configuration.alias ? { ...config, header: e.target.value as string } : config,
		);

		this.dispatchEvent(new UmbChangeEvent());
	}

	#onChangeNameTemplate(e: UUIInputEvent, configuration: UmbCollectionColumnConfiguration) {
		this.value = this.value?.map(
			(config): UmbCollectionColumnConfiguration =>
				config.alias === configuration.alias ? { ...config, nameTemplate: e.target.value as string } : config,
		);

		this.dispatchEvent(new UmbChangeEvent());
	}

	#onRemove(unique: string) {
		const newValue: Array<UmbCollectionColumnConfiguration> = [];

		this.value?.forEach((config) => {
			if (config.alias !== unique) newValue.push(config);
		});

		this.value = newValue;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<div id="layout-wrapper">${this.#renderColumns()}</div>
			${this.#renderInput()}
		`;
	}

	#renderInput() {
		return html`
			<umb-input-collection-content-type-property
				document-types
				media-types
				@change=${this.#onAdd}></umb-input-collection-content-type-property>
		`;
	}

	#renderColumns() {
		if (!this.value) return nothing;
		return repeat(
			this.value,
			(column) => column.alias,
			(column) => this.#renderColumn(column),
		);
	}

	#renderColumn(column: UmbCollectionColumnConfiguration) {
		return html`
			<div class="layout-item" id=${column.alias}>
				<uui-icon name="icon-navigation"></uui-icon>

				<uui-input
					required
					label="label"
					placeholder="Enter a label..."
					.value=${column.header ?? ''}
					@change=${(e: UUIInputEvent) => this.#onChangeLabel(e, column)}></uui-input>

				<div class="alias">
					<code>${column.alias}</code>
				</div>

				<uui-input
					label="template"
					placeholder="Enter a label template..."
					.value=${column.nameTemplate ?? ''}
					@change=${(e: UUIInputEvent) => this.#onChangeNameTemplate(e, column)}></uui-input>

				<div class="actions">
					<uui-button
						label=${this.localize.term('general_remove')}
						look="secondary"
						@click=${() => this.#onRemove(column.alias)}></uui-button>
				</div>
			</div>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#layout-wrapper {
				display: flex;
				flex-direction: column;
				gap: 1px;
				margin-bottom: var(--uui-size-1);
			}

			.layout-item {
				background-color: var(--uui-color-surface-alt);
				display: flex;
				align-items: center;
				gap: var(--uui-size-6);
				padding: var(--uui-size-3) var(--uui-size-6);
			}

			.layout-item > uui-icon {
				flex: 0 0 var(--uui-size-6);
			}

			.layout-item > uui-input,
			.layout-item > .alias {
				flex: 1;
			}

			.layout-item > .actions {
				flex: 0 0 auto;
				display: flex;
				justify-content: flex-end;
			}
		`,
	];
}

export default UmbPropertyEditorUICollectionColumnConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-collection-column-configuration': UmbPropertyEditorUICollectionColumnConfigurationElement;
	}
}
