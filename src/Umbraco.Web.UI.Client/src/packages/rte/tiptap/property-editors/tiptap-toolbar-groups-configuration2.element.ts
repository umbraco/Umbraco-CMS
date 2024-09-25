import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { customElement, css, html, property, repeat, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { Observable, UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

type Extension = {
	alias: string;
	label: string;
	icon?: string;
};

type TestServerValue = [
	{
		alias: string;
		position: [number, number, number];
	},
];

@customElement('umb-tiptap-toolbar-groups-configuration2')
export class UmbTiptapToolbarGroupsConfiguration2Element extends UmbLitElement {
	#testData: TestServerValue = [
		{
			alias: 'bold',
			position: [0, 0, 0],
		},
		{
			alias: 'italic',
			position: [0, 0, 1],
		},
		{
			alias: 'undo',
			position: [0, 1, 0],
		},
		{
			alias: 'redo',
			position: [0, 1, 1],
		},
		{
			alias: 'copy',
			position: [1, 0, 0],
		},
		{
			alias: 'paste',
			position: [1, 2, 0],
		},
	];

	toStructuredData = (data: any): string[][][] => {
		const structuredData: string[][][] = [];

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

	override render() {
		return html``;
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

export default UmbTiptapToolbarGroupsConfiguration2Element;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tiptap-toolbar-groups-configuration2': UmbTiptapToolbarGroupsConfiguration2Element;
	}
}
