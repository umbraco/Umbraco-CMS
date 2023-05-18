import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { groupBy } from 'lodash-es';
import type { UUIInputEvent } from '@umbraco-ui/uui';
import {
	UmbPropertyEditorUIPickerModalData,
	UmbPropertyEditorUIPickerModalResult,
	UmbModalHandler,
} from '@umbraco-cms/backoffice/modal';
import { ManifestPropertyEditorUI, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

interface GroupedPropertyEditorUIs {
	[key: string]: Array<ManifestPropertyEditorUI>;
}
@customElement('umb-data-type-picker-flow-modal')
export class UmbDataTypePickerFlowModalElement extends UmbLitElement {
	@property({ attribute: false })
	modalHandler?: UmbModalHandler<UmbPropertyEditorUIPickerModalData, UmbPropertyEditorUIPickerModalResult>;

	@property({ type: Object })
	data?: UmbPropertyEditorUIPickerModalData;

	@state()
	private _groupedPropertyEditorUIs: GroupedPropertyEditorUIs = {};

	@state()
	private _selection: Array<string> = [];

	@state()
	private _submitLabel = 'Select';

	#propertyEditorUIs: Array<ManifestPropertyEditorUI> = [];
	#currentFilterQuery = '';

	connectedCallback(): void {
		super.connectedCallback();

		this._selection = this.data?.selection ?? [];
		this._submitLabel = this.data?.submitLabel ?? this._submitLabel;

		this._usePropertyEditorUIs();
	}

	private _usePropertyEditorUIs() {
		if (!this.data) return;

		this.observe(umbExtensionsRegistry.extensionsOfType('propertyEditorUI'), (propertyEditorUIs) => {
			// TODO: this should use same code as querying.
			this.#propertyEditorUIs = propertyEditorUIs;
			this._performFiltering();
		});
	}

	private _handleClick(propertyEditorUI: ManifestPropertyEditorUI) {
		this._select(propertyEditorUI.alias);
	}

	private _select(alias: string) {
		this._selection = [alias];
	}

	private _handleFilterInput(event: UUIInputEvent) {
		const query = (event.target.value as string) || '';
		this.#currentFilterQuery = query.toLowerCase();
		this._performFiltering();
	}
	private _performFiltering() {
		const result = !this.#currentFilterQuery
			? this.#propertyEditorUIs
			: this.#propertyEditorUIs.filter((propertyEditorUI) => {
					return (
						propertyEditorUI.name.toLowerCase().includes(this.#currentFilterQuery) ||
						propertyEditorUI.alias.toLowerCase().includes(this.#currentFilterQuery)
					);
			  });

		// TODO: When filtering, then we should also display the available data types, in a separate list as the UIs.

		this._groupedPropertyEditorUIs = groupBy(result, 'meta.group');
	}

	private _close() {
		this.modalHandler?.reject();
	}

	private _submit() {
		this.modalHandler?.submit({ selection: this._selection });
	}

	render() {
		return html`
			<umb-body-layout headline="Select Property Editor">
				<uui-box> ${this._renderFilter()} ${this._renderGrid()} </uui-box>
				<div slot="actions">
					<uui-button label="Close" @click=${this._close}></uui-button>
					<uui-button label="${this._submitLabel}" look="primary" color="positive" @click=${this._submit}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	private _renderFilter() {
		return html` <uui-input
			id="filter"
			@input="${this._handleFilterInput}"
			placeholder="Type to filter..."
			label="Type to filter icons">
			<uui-icon name="search" slot="prepend" id="filter-icon"></uui-icon>
		</uui-input>`;
	}

	private _renderGrid() {
		return html` ${Object.entries(this._groupedPropertyEditorUIs).map(
			([key, value]) =>
				html` <h4>${key}</h4>
					${this._renderGroupItems(value)}`
		)}`;
	}

	private _renderGroupItems(groupItems: Array<ManifestPropertyEditorUI>) {
		return html` <ul id="item-grid">
			${repeat(
				groupItems,
				(propertyEditorUI) => propertyEditorUI.alias,
				(propertyEditorUI) => html` <li class="item" ?selected=${this._selection.includes(propertyEditorUI.alias)}>
					<button type="button" @click="${() => this._handleClick(propertyEditorUI)}">
						<uui-icon name="${propertyEditorUI.meta.icon}" class="icon"></uui-icon>
						${propertyEditorUI.meta.label || propertyEditorUI.name}
					</button>
				</li>`
			)}
		</ul>`;
	}

	static styles = [
		UUITextStyles,
		css`
			#filter {
				width: 100%;
				margin-bottom: var(--uui-size-space-4);
			}

			#filter-icon {
				padding-left: var(--uui-size-space-2);
			}

			#item-grid {
				display: grid;
				grid-template-columns: repeat(auto-fill, minmax(70px, 1fr));
				margin: 0;
				padding: 0;
				grid-gap: var(--uui-size-space-4);
			}

			#item-grid .item {
				display: flex;
				align-items: flex-start;
				justify-content: center;
				list-style: none;
				height: 100%;
				border: 1px solid transparent;
				border-radius: var(--uui-border-radius);
			}

			#item-grid .item:hover {
				background: var(--uui-color-surface-emphasis);
				color: var(--uui-color-interactive-emphasis);
				cursor: pointer;
			}

			#item-grid .item[selected] button {
				background: var(--uui-color-selected);
				color: var(--uui-color-selected-contrast);
			}

			#item-grid .item button {
				background: none;
				border: none;
				cursor: pointer;
				padding: var(--uui-size-space-3);
				display: flex;
				align-items: center;
				flex-direction: column;
				justify-content: center;
				font-size: 0.8rem;
				height: 100%;
				width: 100%;
				color: var(--uui-color-interactive);
				border-radius: var(--uui-border-radius);
			}

			#item-grid .item .icon {
				font-size: 2em;
				margin-bottom: var(--uui-size-space-2);
			}
		`,
	];
}

export default UmbDataTypePickerFlowModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-data-type-picker-flow-modal': UmbDataTypePickerFlowModalElement;
	}
}
