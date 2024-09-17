import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { customElement, css, html, property, state, repeat, render } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import {
	UmbPropertyValueChangeEvent,
	type UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';

import { tinymce } from '@umbraco-cms/backoffice/external/tinymce';

const tinyIconSet = tinymce.IconManager.get('default');

type ToolbarConfig = {
	alias: string;
	label: string;
	icon?: string;
	selected: boolean;
	group: string;
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

		this._selectedValuesNew = [
			[
				[
					{
						alias: 'undo',
						label: 'Undo',
						icon: 'undo',
						selected: false,
						group: 'clipboard',
					},
				],
			],
			[[]],
		];

		this.#selectedValues.forEach((alias) => {
			const row = Math.floor(Math.random() * 2);
			const group = Math.floor(Math.random() * 2);
			const item = this._toolbarConfig.find((value) => value.alias === alias);

			if (!item) return;

			// Ensure the row exists
			if (!this._selectedValuesNew[row]) {
				this._selectedValuesNew[row] = []; // Initialize the row if it doesn't exist
			}

			// Ensure the group exists within the row
			if (!this._selectedValuesNew[row][group]) {
				this._selectedValuesNew[row][group] = []; // Initialize the group if it doesn't exist
			}

			// Add the item to the selectedValuesNew array
			this._selectedValuesNew[row][group].push(item);
		});

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

	#selectedValues: string[] = [];

	@state()
	_selectedValuesNew: ToolbarConfig[][][] = [[[]]];

	protected override async firstUpdated(_changedProperties: PropertyValueMap<unknown>) {
		super.firstUpdated(_changedProperties);

		this.config?.getValueByAlias<ToolbarConfig[]>('toolbar')?.forEach((v) => {
			this._toolbarConfig.push({
				...v,
				selected: this.value.includes(v.alias),
			});
		});

		const grouped = this._toolbarConfig.reduce((acc: any, item) => {
			const group = item.group || 'miscellaneous'; // Assign to "miscellaneous" if no group

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
		const element = event.target as HTMLElement;
		if (!element) return;
		element.classList.add('drag-over');
	};

	#onDragEnd(event: DragEvent) {
		const element = event.target as HTMLElement;
		if (!element) return;
		element.classList.remove('drag-over');
	}

	#onDrop(event: DragEvent) {
		event.preventDefault();
		const groupElement = event.target as HTMLElement;
		if (!groupElement) return;

		groupElement.classList.remove('drag-over');

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
	}

	#renderRow(row: ToolbarConfig[][], rowIndex: number) {
		return html`<div class="selected-row">
			${row.map((group, index) => {
				return this.#renderGroup(group, index, rowIndex);
			})}
			<uui-button look="secondary" compact @click=${() => this.#addGroup(rowIndex)}>+</uui-button>
		</div>`;
	}

	#renderGroup(group: ToolbarConfig[], groupIndex: number, rowIndex: number) {
		return html`<div
			class="selected-group"
			umb-data-group=${groupIndex}
			umb-data-row=${rowIndex}
			@dragover=${this.#onDragOver}
			@dragend=${this.#onDragEnd}
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
		console.log('RENDER');
		return html`
			<div class="selected-bar">
				${repeat(this._selectedValuesNew, (row, index) => this.#renderRow(row, index))}
				<uui-button look="secondary" compact @click=${() => this.#addRow()}>+</uui-button>
			</div>
			<div class="extensions">
				${repeat(
					this._toolbarItems,
					(group) => html`
						<p class="group-name">${group.name}</p>
						${repeat(
							group.items,
							(item) =>
								html`<uui-button
									compact
									look="outline"
									class=${item.selected ? 'selected' : ''}
									label=${item.label}
									.value=${item.alias}
									@click=${() => this.#onExtensionSelect(item)}
									><uui-icon .svg=${tinyIconSet?.icons[item.icon ?? 'alignjustify']}></uui-icon
								></uui-button>`,
						)}
					`,
				)}
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
			.selected-bar {
				display: flex;
				flex-direction: column;
				gap: 12px;
			}
			.selected-row {
				display: flex;
				gap: 18px;
			}
			.selected-group {
				padding: 6px;
				min-width: 12px;
				background-color: var(--uui-color-surface-alt);
				border-radius: var(--uui-border-radius);
				display: flex;
				gap: 6px;
			}
			.selected-group.drag-over uui-button {
				pointer-events: none;
			}
			.extensions {
				display: grid;
				grid-template-columns: repeat(auto-fit, 36px);
				gap: 10px;
			}
			.group-name {
				grid-column: 1 / -1;
				margin-bottom: 0;
				font-weight: bold;
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
