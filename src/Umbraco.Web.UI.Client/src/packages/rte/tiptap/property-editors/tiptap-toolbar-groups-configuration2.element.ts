import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { customElement, css, html, property, repeat, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { Observable, UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

type Extension = {
	alias: string;
	label: string;
	icon?: string;
};

type TestServerValue = Array<{
	alias: string;
	position: [number, number, number];
}>;

@customElement('umb-tiptap-toolbar-groups-configuration2')
export class UmbTiptapToolbarGroupsConfiguration2Element extends UmbLitElement {
	#testData: TestServerValue = [
		{
			alias: 'asdasd',
			position: [0, 0, 0],
		},
		{
			alias: 'itafsdgsdglic',
			position: [0, 0, 1],
		},
		{
			alias: 'sdgsdg',
			position: [0, 1, 0],
		},
		{
			alias: 'asdasd',
			position: [0, 1, 1],
		},
		{
			alias: 'afasf',
			position: [1, 0, 0],
		},
		{
			alias: 'sdgsdg',
			position: [1, 2, 0],
		},
	];

	@property({ attribute: false })
	set value(value: TestServerValue) {
		if (this.#originalFormat === value) return;
		this.#originalFormat = value;
		this._structuredData = this.toStructuredData(value);
	}

	get value(): TestServerValue {
		console.log('get value groups');
		return this.#originalFormat;
	}

	@state()
	_structuredData: string[][][] = [[[]]];

	#originalFormat: TestServerValue = [];

	toStructuredData = (data: TestServerValue) => {
		console.log('toStructuredData');
		const structuredData: string[][][] = [];

		if (!data.length) return [[[]]];

		data.forEach(({ alias, position }) => {
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
	};

	toOriginalFormat = (structuredData: string[][][]) => {
		console.log('toOriginalFormat');
		const originalData: any = [];

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

		return originalData;
	};
	private renderItem(alias: string) {
		return html`<div class="item" draggable="true">${alias}</div>`;
	}

	private renderGroup(group: string[]) {
		return html` <div class="group" dropzone="move">${group.map((alias) => this.renderItem(alias))}</div> `;
	}

	private renderRow(row: string[][]) {
		return html` <div class="row">${repeat(row, (group) => this.renderGroup(group))}</div> `;
	}

	override render() {
		return html`${repeat(this._structuredData, (row) => this.renderRow(row))}`;
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
				border: 1px solid var(--uui-color-border);
				padding: 6px;
				min-height: 24px;
				min-width: 24px;
			}
			.item {
				padding: 3px;
				border: 1px solid var(--uui-color-border);
				border-radius: 3px;
				background-color: var(--uui-color-surface);
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

export default UmbTiptapToolbarGroupsConfiguration2Element;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tiptap-toolbar-groups-configuration2': UmbTiptapToolbarGroupsConfiguration2Element;
	}
}
