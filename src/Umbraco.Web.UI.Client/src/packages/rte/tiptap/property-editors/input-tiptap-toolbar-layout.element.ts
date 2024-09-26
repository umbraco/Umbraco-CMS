import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { customElement, css, html, property, repeat, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

type Extension = {
	alias: string;
	label: string;
	icon?: string;
};

type TestServerValue = Array<{
	alias: string;
	position?: [number, number, number];
}>;

@customElement('umb-input-tiptap-toolbar-layout')
export class UmbTiptapToolbarGroupsConfigurationElement extends UmbLitElement {
	@property({ attribute: false })
	set value(value: TestServerValue) {
		if (this.#originalFormat === value) return;
		// TODO: also check if the added values have positions, if not, there's no need to update the structured data.
		this.#originalFormat = value;
		this._structuredData = this.#toStructuredData(value);
	}

	get value(): TestServerValue {
		return this.#originalFormat;
	}

	@property({ attribute: false })
	extensionConfigs: Extension[] = [];

	@state()
	_structuredData: string[][][] = [[[]]];

	#originalFormat: TestServerValue = [];

	#currentDragAlias?: string;

	#onDragStart = (event: DragEvent, alias: string) => {
		this.#currentDragAlias = alias;
		event.dataTransfer!.effectAllowed = 'move';
	};

	#onDragOver = (event: DragEvent) => {
		event.preventDefault();
		event.dataTransfer!.dropEffect = 'move';
	};

	#onDragEnd = (event: DragEvent) => {
		event.preventDefault();
		if (event.dataTransfer?.dropEffect === 'none') {
			const fromPos = this.#originalFormat.find((item) => item.alias === this.#currentDragAlias)?.position;
			if (!fromPos) return;

			this.#moveItemToHiddenExtensions(fromPos);
		}
	};

	#onDrop = (event: DragEvent, toPos: [number, number, number]) => {
		event.preventDefault();
		const fromPos = this.#originalFormat.find((item) => item.alias === this.#currentDragAlias)?.position;

		if (fromPos) {
			this.#moveItem(fromPos, toPos);
		} else if (this.#currentDragAlias) {
			this.#insertItem(this.#currentDragAlias, toPos);
		}
	};

	#moveItem = (from: [number, number, number], to: [number, number, number]) => {
		const [rowIndex, groupIndex, itemIndex] = from;

		// Get the item to move from the 'from' position
		const itemToMove = this._structuredData[rowIndex][groupIndex][itemIndex];

		// Remove the item from the original position
		this._structuredData[rowIndex][groupIndex].splice(itemIndex, 1);

		this.#insertItem(itemToMove, to);
	};

	#insertItem = (alias: string, toPos: [number, number, number]) => {
		const [rowIndex, groupIndex, itemIndex] = toPos;
		// Insert the item into the new position
		this._structuredData[rowIndex][groupIndex].splice(itemIndex, 0, alias);
		this.#updateOriginalFormat();

		this.requestUpdate('_structuredData');
		this.dispatchEvent(new UmbChangeEvent());
	};

	#moveItemToHiddenExtensions(from: [number, number, number]) {
		const [rowIndex, groupIndex, itemIndex] = from;
		this._structuredData[rowIndex][groupIndex].splice(itemIndex, 1);

		this.#updateOriginalFormat();

		this.requestUpdate('_structuredData');
		this.dispatchEvent(new UmbChangeEvent());
	}

	#addGroup = (rowIndex: number, groupIndex: number) => {
		this._structuredData[rowIndex].splice(groupIndex, 0, []);
		this.requestUpdate('_structuredData');
	};

	#removeGroup = (rowIndex: number, groupIndex: number) => {
		if (rowIndex === 0 && groupIndex === 0) {
			// Prevent removing the last group
			this._structuredData[rowIndex][groupIndex] = [];
		} else {
			this._structuredData[rowIndex].splice(groupIndex, 1);
		}
		this.requestUpdate('_structuredData');
		this.#updateOriginalFormat();
	};

	#addRow = (rowIndex: number) => {
		this._structuredData.splice(rowIndex, 0, [[]]);
		this.requestUpdate('_structuredData');
	};

	#removeRow = (rowIndex: number) => {
		if (rowIndex === 0) {
			// Prevent removing the last row
			this._structuredData[rowIndex] = [[]];
		} else {
			this._structuredData.splice(rowIndex, 1);
		}
		this.requestUpdate('_structuredData');
		this.#updateOriginalFormat();
	};

	#updateOriginalFormat() {
		this.#originalFormat = this.#toOriginalFormat(this._structuredData);
		this.dispatchEvent(new UmbChangeEvent());
	}

	#renderItem(alias: string) {
		const extension = this.extensionConfigs.find((ext) => ext.alias === alias);
		if (!extension) return nothing;
		return html`<div
			class="item"
			draggable="true"
			@dragend=${this.#onDragEnd}
			@dragstart=${(e: DragEvent) => this.#onDragStart(e, alias)}>
			<umb-icon name=${extension.icon ?? ''}></umb-icon>
		</div>`;
	}

	#renderGroup(group: string[], rowIndex: number, groupIndex: number) {
		return html`
			<div
				class="group"
				dropzone="move"
				@dragover=${this.#onDragOver}
				@drop=${(e: DragEvent) => this.#onDrop(e, [rowIndex, groupIndex, group.length])}>
				${group.map((alias) => this.#renderItem(alias))}
				<uui-button
					look="primary"
					color="danger"
					compact
					class="remove-group-button ${rowIndex === 0 && groupIndex === 0 && group.length === 0 ? 'hidden' : undefined}"
					@click=${() => this.#removeGroup(rowIndex, groupIndex)}>
					<umb-icon name="icon-trash"></umb-icon>
				</uui-button>
			</div>
		`;
	}

	#renderRow(row: string[][], rowIndex: number) {
		return html`
			<div class="row">
				${repeat(row, (group, groupIndex) => this.#renderGroup(group, rowIndex, groupIndex))}
				<uui-button look="secondary" @click=${() => this.#addGroup(rowIndex, row.length)}>+</uui-button>
				<uui-button
					look="primary"
					color="danger"
					compact
					class="remove-row-button ${rowIndex === 0 && row[rowIndex].length === 0 ? 'hidden' : undefined}"
					@click=${() => this.#removeRow(rowIndex)}>
					<umb-icon name="icon-trash"></umb-icon>
				</uui-button>
			</div>
		`;
	}

	override render() {
		return html`
			<p style="margin-top: 0">
				<uui-tag color="warning">WIP Feature</uui-tag> Rows, groups, and item order have no effect yet. <br />
				However, adding and removing items from the toolbar is functional. Additionally, hiding items from the toolbar
				while retaining their functionality by excluding them from the toolbar layout is also functional.
			</p>
			${repeat(this._structuredData, (row, rowIndex) => this.#renderRow(row, rowIndex))}
			<uui-button look="secondary" @click=${() => this.#addRow(this._structuredData.length)}>+</uui-button>
			${this.#renderHiddenExtensions()}
		`;
	}

	#renderHiddenExtensions() {
		const hiddenExtensions = this.#originalFormat?.filter((item) => !item.position);

		if (!hiddenExtensions?.length) return nothing;

		return html`
			<p class="hidden-extensions-header">
				Extensions hidden from the toolbar<br />Drag and drop buttons into the toolbar to add them
			</p>

			<div class="hidden-extensions">${hiddenExtensions.map((item) => this.#renderItem(item.alias))}</div>
		`;
	}

	#toStructuredData(data: TestServerValue) {
		if (!data?.length) return [[[]]];

		const structuredData: string[][][] = [[[]]];
		data.forEach(({ alias, position }) => {
			if (!position) return;

			const [rowIndex, groupIndex, aliasIndex] = position;

			while (structuredData.length <= rowIndex) {
				structuredData.push([]);
			}

			const currentRow = structuredData[rowIndex];

			while (currentRow.length <= groupIndex) {
				currentRow.push([]);
			}

			const currentGroup = currentRow[groupIndex];

			currentGroup[aliasIndex] = alias;
		});

		return structuredData;
	}

	#toOriginalFormat = (structuredData: string[][][]) => {
		const originalData: TestServerValue = [];

		structuredData.forEach((row, rowIndex) => {
			row.forEach((group, groupIndex) => {
				group.forEach((alias, aliasIndex) => {
					if (alias) {
						originalData.push({
							alias,
							position: [rowIndex, groupIndex, aliasIndex],
						});
					}
				});
			});
		});

		// Add the hidden extensions so they are not lost
		this.#originalFormat.forEach((item) => {
			if (!originalData.some((i) => i.alias === item.alias)) {
				originalData.push({
					alias: item.alias,
				});
			}
		});

		return originalData;
	};

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				gap: 6px;
			}
			.hidden-extensions {
				display: flex;
				gap: 6px;
			}
			.hidden-extensions-header {
				margin-bottom: 3px;
			}
			.row {
				position: relative;
				display: flex;
				gap: 12px;
			}
			.group {
				position: relative;
				display: flex;
				gap: 3px;
				border-radius: var(--uui-border-radius);
				background-color: var(--uui-color-surface-alt);
				padding: 6px;
				min-height: 30px;
				min-width: 30px;
			}
			.item {
				padding: var(--uui-size-space-2);
				border: 1px solid var(--uui-color-border);
				border-radius: var(--uui-border-radius);
				background-color: var(--uui-color-surface);
				cursor: move;
				display: flex;
				align-items: baseline;
			}

			.remove-row-button,
			.remove-group-button {
				display: none;
			}
			.remove-group-button {
				position: absolute;
				top: -26px;
				left: 50%;
				transform: translateX(-50%);
				z-index: 1;
			}

			.row:hover .remove-row-button:not(.hidden),
			.group:hover .remove-group-button:not(.hidden) {
				display: flex;
			}
		`,
	];
}

export default UmbTiptapToolbarGroupsConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-tiptap-toolbar-layout': UmbTiptapToolbarGroupsConfigurationElement;
	}
}
