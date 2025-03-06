import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { TemplateResult } from '@umbraco-cms/backoffice/external/lit';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

@customElement('umb-tiptap-table-insert')
export class UmbTiptapTableInsertElement extends UmbLitElement {
	editor?: Editor;

	@state()
	private _selectedColumn = 0;

	@state()
	private _selectedRow = 0;

	#onClick(column: number, row: number) {
		this._selectedColumn = column;
		this._selectedRow = row;

		this.editor?.chain().focus().insertTable({ rows: row, cols: column }).run();
	}

	#onMouseover(column: number, row: number) {
		this._selectedColumn = column;
		this._selectedRow = row;
	}

	override render() {
		const rows = 10;
		const columns = 10;

		const cells: Array<Array<TemplateResult>> = [];

		for (let i = 1; i <= rows; i++) {
			const row = [];
			for (let j = 1; j <= columns; j++) {
				row.push(html`
					<button
						type="button"
						class=${i <= this._selectedRow && j <= this._selectedColumn ? 'selected' : ''}
						aria-label="${j} columns, ${i} rows"
						@mouseover=${() => this.#onMouseover(j, i)}
						@click=${() => this.#onClick(j, i)}></button>
				`);
			}
			cells.push(row);
		}

		return html`
			<div id="grid">${cells}</div>
			<label>${this._selectedColumn} &times; ${this._selectedRow}</label>
		`;
	}

	static override readonly styles = [
		css`
			:host {
				background-color: var(--uui-color-surface);
				display: flex;
				flex-direction: column;
				justify-content: center;
				align-items: center;
				width: 170px;
			}

			#grid {
				display: flex;
				flex-wrap: wrap;

				> button {
					all: unset;
					box-sizing: border-box;

					border-color: var(--uui-color-border);
					border-style: solid;
					border-width: 0 1px 1px 0;

					height: 17px;
					width: 17px;

					&:hover,
					&.selected {
						background-color: var(--uui-color-background);
						border-color: var(--uui-color-selected);
					}
				}
			}
		`,
	];
}

export default UmbTiptapTableInsertElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tiptap-table-insert': UmbTiptapTableInsertElement;
	}
}
