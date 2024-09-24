import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { customElement, css, html, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import {
	UmbPropertyValueChangeEvent,
	type UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';

import './tiptap-toolbar-groups-configuration.element.js';

import { tinymce } from '@umbraco-cms/backoffice/external/tinymce';

const tinyIconSet = tinymce.IconManager.get('default');

type ToolbarConfig = {
	alias: string;
	label: string;
	icon?: string;
	selected: boolean;
	category: string;
};

type ToolbarItems = Array<{
	name: string;
	items: ToolbarConfig[];
}>;

/**
 * @element umb-property-editor-ui-tiptap-toolbar-configuration
 */
@customElement('umb-property-editor-ui-tiptap-toolbar-configuration')
export class UmbPropertyEditorUiTiptapToolbarConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	@property({ attribute: false })
	set value(value: string | string[] | null) {
		if (!value) {
			this.#selectedValues = [];
		} else {
			if (typeof value === 'string') {
				this.#selectedValues = value.split(',').filter((x) => x.length > 0);
			} else if (Array.isArray(value)) {
				this.#selectedValues = value;
			} else {
				this.#selectedValues = [];
			}
		}

		this.requestUpdate('#selectedValuesNew');
	}
	get value(): string[] {
		return this.#selectedValues;
	}

	@property({ attribute: false })
	config?: UmbPropertyEditorConfigCollection;

	@state()
	private _toolbarItems: ToolbarItems = [];

	@state()
	private _toolbarConfig: Array<ToolbarConfig> = [];

	@state()
	_selectedValuesNew: ToolbarConfig[][][] = [[[]]];

	#selectedValues: string[] = [];

	#hoveredDropzone: HTMLElement | null = null; // Will be used to sort extensions in a group in the toolbar

	protected override async firstUpdated(_changedProperties: PropertyValueMap<unknown>) {
		super.firstUpdated(_changedProperties);

		this.config?.getValueByAlias<ToolbarConfig[]>('toolbar')?.forEach((v) => {
			this._toolbarConfig.push({
				...v,
				selected: this.value.includes(v.alias),
			});
		});

		const grouped = this._toolbarConfig.reduce((acc: any, item) => {
			const group = item.category || 'miscellaneous'; // Assign to "miscellaneous" if no group

			if (!acc[group]) {
				acc[group] = [];
			}
			acc[group].push(item);
			return acc;
		}, {});

		this._toolbarItems = Object.keys(grouped).map((group) => ({
			name: group.charAt(0).toUpperCase() + group.slice(1).replace(/-/g, ' '),
			items: grouped[group],
		}));

		this.requestUpdate('_toolbarConfig');
	}

	#onExtensionSelect(item: ToolbarConfig, row?: number, group?: number) {
		// if no row is provided, add to the last row and last group
		if (row === undefined) {
			row = this._selectedValuesNew.length - 1;
		}

		// if no group is provided, add to the last group in the row
		if (group === undefined) {
			group = this._selectedValuesNew[row].length - 1;
		}

		// Add the item to the selectedValuesNew array
		this._selectedValuesNew[row][group].push(item);
		this.requestUpdate('_selectedValuesNew');
	}

	#addGroup(row: number) {
		this._selectedValuesNew[row].push([]);
		this.requestUpdate('_selectedValuesNew');
	}

	#addRow() {
		this._selectedValuesNew.push([[]]);
		this.requestUpdate('_selectedValuesNew');
	}

	#onChange = (item: ToolbarConfig) => {
		const value = this._toolbarItems
			.flatMap((group) =>
				group.items.map((i) => {
					if (i.alias === item.alias) {
						i.selected = !i.selected;
					}
					return i.selected ? i.alias : null;
				}),
			)
			.filter((v): v is string => v !== null); // Ensures we only keep non-null strings

		// If the value array is empty, set this.value to null, otherwise assign the array
		this.value = value.length > 0 ? value : null;

		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	};

	#onDragStart = (event: DragEvent, alias: string) => {
		event.dataTransfer!.setData('text/plain', alias);
		event.dataTransfer!.dropEffect = 'move';
		event.dataTransfer!.effectAllowed = 'move';
	};

	#onDragOver = (event: DragEvent) => {
		event.preventDefault();
	};

	#onDragEnter = (event: DragEvent) => {
		const dropzone = event
			.composedPath()
			.find((v) => v instanceof HTMLElement && v.classList.contains('toolbar-group') && v.hasAttribute('dropzone'));

		this.#hoveredDropzone = (dropzone as HTMLElement) || null;
		console.log('hovered dropzone', this.#hoveredDropzone);
	};

	#onDrop = (event: DragEvent) => {
		event.preventDefault();

		const groupElement = event
			.composedPath()
			.find(
				(v) => v instanceof HTMLElement && v.classList.contains('toolbar-group') && v.hasAttribute('dropzone'),
			) as HTMLElement;

		if (!groupElement) return;

		const alias = event.dataTransfer!.getData('text/plain');
		if (!alias) return;

		const item = this._toolbarConfig.find((v) => v.alias === alias);
		if (!item) return;

		const rowAttribute = groupElement.getAttribute('umb-data-row');
		const rowIndex = rowAttribute ? Number.parseInt(rowAttribute) : null;

		const groupAttribute = groupElement.getAttribute('umb-data-group');
		const groupIndex = groupAttribute ? Number.parseInt(groupAttribute) : null;

		if (groupIndex === null || rowIndex === null) return;

		// remove alias from selectedValues
		this._selectedValuesNew = this._selectedValuesNew.map((row) =>
			row.map((group) => group.filter((v) => v.alias !== alias)),
		);

		this.#onExtensionSelect(item, rowIndex, groupIndex);
	};

	#renderRow(row: ToolbarConfig[][], rowIndex: number) {
		return html`<div class="toolbar-row">
			${row.map((group, index) => {
				return this.#renderGroup(group, index, rowIndex);
			})}
			<uui-button look="secondary" @click=${() => this.#addGroup(rowIndex)}>Add group</uui-button>
		</div>`;
	}

	#renderGroup(group: ToolbarConfig[], groupIndex: number, rowIndex: number) {
		return html`<div
			class=${`toolbar-group`}
			umb-data-group=${groupIndex}
			umb-data-row=${rowIndex}
			@dragover=${this.#onDragOver}
			@dragenter=${this.#onDragEnter}
			@drop=${this.#onDrop}
			dropzone="move">
			${group.map((item) => {
				return html`
					<uui-button
						draggable="true"
						@dragstart=${(e: DragEvent) => this.#onDragStart(e, item.alias)}
						compact
						look="outline"
						class=${item.selected ? 'selected' : ''}
						label=${item.label}
						.value=${item.alias}
						@click=${() => this.#onChange(item)}
						><uui-icon .svg=${tinyIconSet?.icons[item.icon ?? 'alignjustify']}></uui-icon
					></uui-button>
				`;
			})}
		</div>`;
	}

	override render() {
		return html`<umb-tiptap-toolbar-groups-configuration></umb-tiptap-toolbar-groups-configuration>`;
		return html`
			<div class="toolbar">
				${repeat(this._selectedValuesNew, (row, index) => this.#renderRow(row, index))}
				<uui-button look="secondary" @click=${() => this.#addRow()}>Add row</uui-button>
			</div>
			<div class="extensions">
				${repeat(
					this._toolbarItems,
					(category) => html`
						<div class="category">
							<p class="category-name">
								${category.name}
								<span style="margin-left: auto; font-size: 0.8em; opacity: 0.5;">Hide in toolbar</span>
							</p>
							${repeat(
								category.items,
								(item) =>
									html`<div class="extension-item">
										<uui-button
											compact
											look="outline"
											class=${item.selected ? 'selected' : ''}
											label=${item.label}
											.value=${item.alias}
											@click=${() => this.#onExtensionSelect(item)}
											><uui-icon .svg=${tinyIconSet?.icons[item.icon ?? 'alignjustify']}></uui-icon
										></uui-button>
										<span>${item.label}</span>
										<uui-checkbox aria-label="Hide in toolbar"></uui-checkbox>
									</div>`,
							)}
						</div>
					`,
				)}
					</div>
			</div>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			uui-icon {
				width: unset;
				height: unset;
				display: flex;
				vertical-align: unset;
			}
			uui-button.selected {
				--uui-button-border-color: var(--uui-color-selected);
				--uui-button-border-width: 2px;
			}
			.toolbar {
				display: flex;
				flex-direction: column;
				gap: 12px;
			}
			.toolbar-row {
				display: flex;
				gap: 18px;
			}
			.toolbar-group {
				padding: 6px;
				min-width: 12px;
				background-color: var(--uui-color-surface-alt);
				border-radius: var(--uui-border-radius);
				display: flex;
				gap: 6px;
				min-width: 24px;
			}
			.extensions {
				display: grid;
				grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
				gap: 16px;
				margin-top: 16px;
			}
			.extension-item {
				display: grid;
				grid-template-columns: 36px 1fr auto;
				grid-template-rows: 1fr;
				align-items: center;
				gap: 9px;
			}
			.category {
				background-color: var(--uui-color-surface-alt);
				padding: 12px;
				border-radius: 6px;
				display: flex;
				flex-direction: column;
				gap: 6px;
				border: 1px solid var(--uui-color-border);
			}
			.category-name {
				grid-column: 1 / -1;
				margin: 0;
				font-weight: bold;
				display: flex;
			}
		`,
	];
}

export default UmbPropertyEditorUiTiptapToolbarConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tiptap-toolbar-configuration': UmbPropertyEditorUiTiptapToolbarConfigurationElement;
	}
}
