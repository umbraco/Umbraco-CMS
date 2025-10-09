import type { ManifestPropertyEditorUi } from '../extensions/types.js';
import type {
	UmbPropertyEditorUIPickerModalData,
	UmbPropertyEditorUIPickerModalValue,
} from './property-editor-ui-picker-modal.token.js';
import { css, customElement, html, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { fromCamelCase } from '@umbraco-cms/backoffice/utils';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { umbFocus } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-property-editor-ui-picker-modal')
export class UmbPropertyEditorUIPickerModalElement extends UmbModalBaseElement<
	UmbPropertyEditorUIPickerModalData,
	UmbPropertyEditorUIPickerModalValue
> {
	@state()
	private _groupedPropertyEditorUIs: Array<{ key: string; items: Array<ManifestPropertyEditorUi> }> = [];

	@state()
	private _propertyEditorUIs: Array<ManifestPropertyEditorUi> = [];

	override connectedCallback(): void {
		super.connectedCallback();

		this.#usePropertyEditorUIs();
	}

	#usePropertyEditorUIs() {
		this.observe(umbExtensionsRegistry.byType('propertyEditorUi'), (propertyEditorUIs) => {
			// Only include Property Editor UIs which has Property Editor Schema Alias
			this._propertyEditorUIs = propertyEditorUIs
				.filter((propertyEditorUi) => !!propertyEditorUi.meta.propertyEditorSchemaAlias)
				.sort((a, b) => a.meta.label.localeCompare(b.meta.label));

			this.#groupPropertyEditorUIs(this._propertyEditorUIs);
		});
	}

	#handleClick(propertyEditorUi: ManifestPropertyEditorUi) {
		this.value = { selection: [propertyEditorUi.alias] };
		this._submitModal();
	}

	#handleFilterInput(event: UUIInputEvent) {
		const query = ((event.target.value as string) || '').toLowerCase();

		const result = !query
			? this._propertyEditorUIs
			: this._propertyEditorUIs.filter(
					(propertyEditorUI) =>
						propertyEditorUI.name.toLowerCase().includes(query) || propertyEditorUI.alias.toLowerCase().includes(query),
				);

		this.#groupPropertyEditorUIs(result);
	}

	#groupPropertyEditorUIs(items: Array<ManifestPropertyEditorUi>) {
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-expect-error
		const grouped = Object.groupBy(items, (propertyEditorUi: ManifestPropertyEditorUi) =>
			fromCamelCase(propertyEditorUi.meta.group),
		);

		this._groupedPropertyEditorUIs = Object.keys(grouped)
			.sort((a, b) => a.localeCompare(b))
			.map((key) => ({ key, items: grouped[key] }));
	}

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('propertyEditorPicker_openPropertyEditorPicker')}>
				<uui-box>${this.#renderFilter()} ${this.#renderGrid()}</uui-box>
				<div slot="actions">
					<uui-button label=${this.localize.term('general_close')} @click=${this._rejectModal}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	#renderFilter() {
		return html`
			<uui-input
				type="search"
				id="filter"
				@input=${this.#handleFilterInput}
				placeholder=${this.localize.term('placeholders_filter')}
				label=${this.localize.term('placeholders_filter')}
				${umbFocus()}>
				<uui-icon name="search" slot="prepend" id="filter-icon"></uui-icon>
			</uui-input>
		`;
	}

	#renderGrid() {
		return html`
			${repeat(
				this._groupedPropertyEditorUIs,
				(group) => group.key,
				(group) => html`
					<h4>${group.key}</h4>
					${this.#renderGroupItems(group.items)}
				`,
			)}
		`;
	}

	#renderGroupItems(groupItems: Array<ManifestPropertyEditorUi>) {
		return html`
			<ul id="item-grid">
				${repeat(
					groupItems,
					(propertyEditorUI) => propertyEditorUI.alias,
					(propertyEditorUI) => html`
						<li class="item" ?selected=${this.value.selection.includes(propertyEditorUI.alias)}>
							<button type="button" @click=${() => this.#handleClick(propertyEditorUI)}>
								<umb-icon name=${propertyEditorUI.meta.icon} class="icon"></umb-icon>
								${propertyEditorUI.meta.label || propertyEditorUI.name}
							</button>
						</li>
					`,
				)}
			</ul>
		`;
	}

	static override styles = [
		css`
			#filter {
				width: 100%;
				margin-bottom: var(--uui-size-space-4);
			}

			#filter-icon {
				height: 100%;
				padding-left: var(--uui-size-space-2);
				display: flex;
				color: var(--uui-color-border);
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

export default UmbPropertyEditorUIPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-picker-modal': UmbPropertyEditorUIPickerModalElement;
	}
}
