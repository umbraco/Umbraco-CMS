import type { UmbInputCollectionContentTypePropertyElement } from './components/index.js';
import { css, customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbCollectionColumnConfiguration } from '@umbraco-cms/backoffice/collection';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import type { UmbSortableListElement } from '@umbraco-cms/backoffice/sorter';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

// import of local components
import './components/index.js';
import '@umbraco-cms/backoffice/sorter';

/**
 * @element umb-property-editor-ui-collection-column-configuration
 */
@customElement('umb-property-editor-ui-collection-column-configuration')
export class UmbPropertyEditorUICollectionColumnConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	@property({ type: Array })
	public set value(value: Array<UmbCollectionColumnConfiguration> | undefined) {
		this.#value = value ?? [];
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
		e.stopPropagation();
		this.value = this.value?.map(
			(config): UmbCollectionColumnConfiguration =>
				config.alias === configuration.alias ? { ...config, header: e.target.value as string } : config,
		);

		this.dispatchEvent(new UmbChangeEvent());
	}

	#onChangeNameTemplate(e: UUIInputEvent, configuration: UmbCollectionColumnConfiguration) {
		e.stopPropagation();
		this.value = this.value?.map(
			(config): UmbCollectionColumnConfiguration =>
				config.alias === configuration.alias ? { ...config, nameTemplate: e.target.value as string } : config,
		);

		this.dispatchEvent(new UmbChangeEvent());
	}

	async #onRemove(column: UmbCollectionColumnConfiguration, index: number) {
		await umbConfirmModal(this, {
			color: 'danger',
			headline: `#actions_remove?`,
			content: `#defaultdialogs_confirmremove ${column.header}?`,
			confirmLabel: '#actions_remove',
		});

		const values = [...(this.value ?? [])];
		values.splice(index, 1);
		this.value = values;

		this.dispatchEvent(new UmbChangeEvent());
	}

	#onSort(e: Event) {
		this.value = (e.target as UmbSortableListElement<UmbCollectionColumnConfiguration>).items;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<umb-sortable-list
				.items=${this.value}
				.getUnique=${(column: UmbCollectionColumnConfiguration) => column.alias}
				.renderMethod=${(column: UmbCollectionColumnConfiguration, index: number) => this.#renderColumn(column, index)}
				@change=${this.#onSort}>
			</umb-sortable-list>
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

	#renderColumn(column: UmbCollectionColumnConfiguration, index: number) {
		return html`
			<umb-sortable-list-item .unique=${column.alias}>
				<div style="display:flex;align-items:center;gap:var(--uui-size-6);">
					<uui-input
						required
						label="label"
						placeholder="Enter a label..."
						style="flex:1;"
						.value=${column.header ?? ''}
						@change=${(e: UUIInputEvent) => this.#onChangeLabel(e, column)}>
					</uui-input>
					<div class="alias" style="flex:1;word-break:break-all;">
						<code>${column.alias}</code>
					</div>
					<uui-input
						label="template"
						placeholder="Enter a label template..."
						style="flex:1;"
						.value=${column.nameTemplate ?? ''}
						@change=${(e: UUIInputEvent) => this.#onChangeNameTemplate(e, column)}>
					</uui-input>
				</div>
				<uui-action-bar slot="actions">
					<uui-button label=${this.localize.term('general_remove')} @click=${() => this.#onRemove(column, index)}>
						<uui-icon name="icon-trash"></uui-icon>
					</uui-button>
				</uui-action-bar>
			</umb-sortable-list-item>
		`;
	}

	static override readonly styles = [
		css`
			umb-sortable-list {
				display: block;
				margin-bottom: var(--uui-size-1);
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
