import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { customElement, css, html, property, repeat, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

type ToolbarLayout = Array<Array<string>>;

type Extension = {
	alias: string;
	label: string;
	icon?: string;
};

@customElement('umb-tiptap-toolbar-groups-configuration')
export class UmbTiptapToolbarGroupsConfigurationElement extends UmbLitElement {
	@property({ attribute: false })
	set value(value: Array<ToolbarLayout>) {
		// TODO: if value is null or does not have at least one row with one group, default to a single row with a single empty group
		// this.#value = value;
		// this.requestUpdate('#value');
	}
	get value(): Array<ToolbarLayout> {
		return this.#value;
	}

	#value: Array<ToolbarLayout> = [
		[['bold', 'italic'], []],
		[[], ['underline'], ['strikethrough']],
	];

	@property({ attribute: false })
	extensions: Array<Extension> = [
		{
			alias: 'bold',
			label: 'Bold',
		},
		{
			alias: 'italic',
			label: 'Italic',
		},
		{
			alias: 'underline',
			label: 'Underline',
		},
		{
			alias: 'strikethrough',
			label: 'Strikethrough',
		},
	];

	private moveItem(from: [number, number, number], to: [number, number, number]) {
		const [fromRow, fromGroup, fromItem] = from;
		const [toRow, toGroup, toItem] = to;

		// Get the item to move from the 'from' position
		const itemToMove = this.#value[fromRow][fromGroup][fromItem];

		// Remove the item from the original position
		this.#value[fromRow][fromGroup].splice(fromItem, 1);

		// Insert the item into the new position
		this.#value[toRow][toGroup].splice(toItem, 0, itemToMove);

		// Trigger an update to re-render the UI
		this.requestUpdate('value');
	}

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
		const extension = this.extensions.find((ext) => ext.alias === alias);
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
				<!-- <div class="remove-group-button">X</div> -->
			</div>
		`;
	}

	private renderRow(row: string[][], rowIndex: number) {
		return html`
			<div class="row">
				${repeat(row, (group, groupIndex) => this.renderGroup(group, rowIndex, groupIndex))}
				<!-- <div class="remove-row-button">Remove row</div> -->
			</div>
		`;
	}

	override render() {
		return html`${repeat(this.#value, (row, rowIndex) => this.renderRow(row, rowIndex))}`;
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
