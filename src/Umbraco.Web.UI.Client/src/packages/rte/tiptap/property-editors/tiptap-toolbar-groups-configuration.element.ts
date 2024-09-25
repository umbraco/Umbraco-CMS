import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { customElement, css, html, property, repeat, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { Observable, UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

type Extension = {
	alias: string;
	label: string;
	icon?: string;
};

@customElement('umb-tiptap-toolbar-groups-configuration')
export class UmbTiptapToolbarGroupsConfigurationElement extends UmbLitElement {
	@property({ attribute: false })
	availableExtensions: Array<Extension> = [];

	@state()
	private _toolbar: string[][][] = [[[]]];

	#toolbarLayout: UmbArrayState<string[][]> | undefined;

	constructor() {
		super();

		this.consumeContext(
			'umb-tiptap-toolbar-context',
			(instance: { state: UmbArrayState<string[][]>; observable: Observable<string[][][]> }) => {
				this.#toolbarLayout = instance.state;

				this.observe(instance.observable, (value) => {
					this._toolbar = value.map((rows) => rows.map((groups) => [...groups]));
				});
			},
		);
	}

	private moveItem = (from: [number, number, number], to: [number, number, number]) => {
		const [fromRow, fromGroup, fromItem] = from;
		const [toRow, toGroup, toItem] = to;

		// Get the item to move from the 'from' position
		const itemToMove = this._toolbar[fromRow][fromGroup][fromItem];

		// Remove the item from the original position
		this._toolbar[fromRow][fromGroup].splice(fromItem, 1);

		// Insert the item into the new position
		this._toolbar[toRow][toGroup].splice(toItem, 0, itemToMove);

		this.#toolbarLayout?.setValue(this._toolbar);
	};

	#addGroup = (rowIndex: number, groupIndex: number) => {
		this._toolbar[rowIndex].splice(groupIndex, 0, []);
		this.#toolbarLayout?.setValue(this._toolbar);
	};

	#removeGroup = (rowIndex: number, groupIndex: number) => {
		this._toolbar[rowIndex].splice(groupIndex, 1);
		this.#toolbarLayout?.setValue(this._toolbar);
	};

	#addRow = (rowIndex: number) => {
		this._toolbar.splice(rowIndex, 0, [[]]);
		this.#toolbarLayout?.setValue(this._toolbar);
	};

	#removeRow = (rowIndex: number) => {
		this._toolbar.splice(rowIndex, 1);
		this.#toolbarLayout?.setValue(this._toolbar);
	};

	#onDragStart = (event: DragEvent, pos: [number, number, number]) => {
		event.dataTransfer!.setData('application/json', JSON.stringify(pos));
		event.dataTransfer!.dropEffect = 'move';
	};

	#onDragOver = (event: DragEvent) => {
		event.preventDefault();
	};

	#onDrop = (event: DragEvent, toPos: [number, number, number]) => {
		event.preventDefault();

		const fromPos: [number, number, number] = JSON.parse(event.dataTransfer!.getData('application/json') ?? '[0,0,0]');
		this.moveItem(fromPos, toPos);
	};

	private renderItem(alias: string, rowIndex: number, groupIndex: number, itemIndex: number) {
		const extension = this.availableExtensions.find((ext) => ext.alias === alias);
		if (!extension) return nothing;

		return html`<div
			class="item"
			draggable="true"
			@dragstart=${(e: DragEvent) => this.#onDragStart(e, [rowIndex, groupIndex, itemIndex])}>
			${extension.label}
		</div>`;
	}

	private renderGroup(group: string[], rowIndex: number, groupIndex: number) {
		return html`
			<div
				class="group"
				dropzone="move"
				@dragover=${this.#onDragOver}
				@drop=${(e: DragEvent) => this.#onDrop(e, [rowIndex, groupIndex, 0])}>
				${group.map((alias, itemIndex) => this.renderItem(alias, rowIndex, groupIndex, itemIndex))}
				<button class="remove-group-button" @click=${() => this.#removeGroup(rowIndex, groupIndex)}>X</button>
			</div>
		`;
	}

	private renderRow(row: string[][], rowIndex: number) {
		return html`
			<div class="row">
				${repeat(row, (group, groupIndex) => this.renderGroup(group, rowIndex, groupIndex))}
				<button @click=${() => this.#addGroup(rowIndex, row.length)}>+</button>
				<button class="remove-row-button" @click=${() => this.#removeRow(rowIndex)}>X</button>
			</div>
		`;
	}

	override render() {
		return html`${repeat(this._toolbar, (row, rowIndex) => this.renderRow(row, rowIndex))}
			<button @click=${() => this.#addRow(this._toolbar.length)}>+</button>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				gap: 6px;
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
				border: 1px solid #ccc;
				padding: 6px;
				min-height: 24px;
				min-width: 24px;
			}
			.item {
				padding: 3px;
				border: 1px solid #ccc;
				border-radius: 3px;
				background-color: #f9f9f9;
			}

			.remove-group-button {
				position: absolute;
				top: -4px;
				right: -4px;
				display: none;
			}
			.group:hover .remove-group-button {
				display: block;
			}

			.remove-row-button {
				position: absolute;
				left: -25px;
				top: 8px;
				display: none;
			}
			.row:hover .remove-row-button {
				display: block;
			}
		`,
	];
}

export default UmbTiptapToolbarGroupsConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tiptap-toolbar-groups-configuration': UmbTiptapToolbarGroupsConfigurationElement;
	}
}
