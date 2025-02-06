import { UmbTiptapExtensionApiBase } from '../base.js';
import { css } from '@umbraco-cms/backoffice/external/lit';
import { Table, TableHeader, TableRow, TableCell } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapTableExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [Table.configure({ resizable: true }), TableHeader, TableRow, TableCell];

	override getStyles = () => css`
		.tableWrapper {
			margin: 1.5rem 0;
			overflow-x: auto;

			table {
				border-collapse: collapse;
				margin: 0;
				overflow: hidden;
				table-layout: fixed;
				width: 100%;

				td,
				th {
					border: 1px solid var(--uui-color-border);
					box-sizing: border-box;
					min-width: 1em;
					padding: 6px 8px;
					position: relative;
					vertical-align: top;

					> * {
						margin-bottom: 0;
					}
				}

				th {
					background-color: var(--uui-color-background);
					font-weight: bold;
					text-align: left;
				}

				.selectedCell:after {
					background: var(--uui-color-surface-emphasis);
					content: '';
					left: 0;
					right: 0;
					top: 0;
					bottom: 0;
					pointer-events: none;
					position: absolute;
					z-index: 2;
				}

				.column-resize-handle {
					background-color: var(--uui-color-default);
					bottom: -2px;
					pointer-events: none;
					position: absolute;
					right: -2px;
					top: 0;
					width: 3px;
				}
			}

			.resize-cursor {
				cursor: ew-resize;
				cursor: col-resize;
			}
		}
	`;
}
