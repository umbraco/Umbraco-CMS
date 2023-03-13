import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { groupBy } from 'lodash-es';
import type { UUIInputEvent } from '@umbraco-ui/uui';
import { UmbPropertyEditorUIPickerModalData, UmbPropertyEditorUIPickerModalResult } from '.';
import type { UmbModalHandler } from '@umbraco-cms/modal';
import type { ManifestPropertyEditorUI } from '@umbraco-cms/models';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-api';
import { UmbLitElement } from '@umbraco-cms/element';

interface GroupedPropertyEditorUIs {
	[key: string]: Array<ManifestPropertyEditorUI>;
}
@customElement('umb-property-editor-ui-picker-modal')
export class UmbPropertyEditorUIPickerModalElement extends UmbLitElement {
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

	@property({ type: Object })
	data?: UmbPropertyEditorUIPickerModalData;

	@state()
	private _groupedPropertyEditorUIs: GroupedPropertyEditorUIs = {};

	@state()
	private _propertyEditorUIs: Array<ManifestPropertyEditorUI> = [];

	@state()
	private _selection: Array<string> = [];

	@state()
	private _submitLabel = 'Select';

	connectedCallback(): void {
		super.connectedCallback();

		this._selection = this.data?.selection ?? [];
		this._submitLabel = this.data?.submitLabel ?? this._submitLabel;

		this._usePropertyEditorUIs();
	}

	private _usePropertyEditorUIs() {
		if (!this.data) return;

		this.observe(umbExtensionsRegistry.extensionsOfType('propertyEditorUI'), (propertyEditorUIs) => {
			this._propertyEditorUIs = propertyEditorUIs;
			this._groupedPropertyEditorUIs = groupBy(propertyEditorUIs, 'meta.group');
		});
	}

	private _handleClick(propertyEditorUI: ManifestPropertyEditorUI) {
		this._select(propertyEditorUI.alias);
	}

	private _select(alias: string) {
		this._selection = [alias];
	}

	private _handleFilterInput(event: UUIInputEvent) {
		let query = (event.target.value as string) || '';
		query = query.toLowerCase();

		const result = !query
			? this._propertyEditorUIs
			: this._propertyEditorUIs.filter((propertyEditorUI) => {
					return (
						propertyEditorUI.name.toLowerCase().includes(query) || propertyEditorUI.alias.toLowerCase().includes(query)
					);
			  });

		this._groupedPropertyEditorUIs = groupBy(result, 'meta.group');
	}

	private _close() {
		this.modalHandler?.reject();
	}

	@property({ attribute: false })
	modalHandler?: UmbModalHandler<UmbPropertyEditorUIPickerModalData, UmbPropertyEditorUIPickerModalResult>;

	private _submit() {
		this.modalHandler?.submit({ selection: this._selection });
	}

	render() {
		return html`
			<umb-workspace-layout headline="Select Property Editor UI">
				<uui-box> ${this._renderFilter()} ${this._renderGrid()} </uui-box>
				<div slot="actions">
					<uui-button label="Close" @click=${this._close}></uui-button>
					<uui-button label="${this._submitLabel}" look="primary" color="positive" @click=${this._submit}></uui-button>
				</div>
			</umb-workspace-layout>
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
}

export default UmbPropertyEditorUIPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-picker-modal': UmbPropertyEditorUIPickerModalElement;
	}
}
