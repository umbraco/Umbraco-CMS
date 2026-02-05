import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { UmbTable, UmbTableHeader, UmbTableRow, UmbTableCell } from './table.tiptap-extension.js';
import { css } from '@umbraco-cms/backoffice/external/lit';

import '../../components/menu/tiptap-menu.element.js';

export default class UmbTiptapTableExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [UmbTable, UmbTableHeader, UmbTableRow, UmbTableCell];

	override getStyles = () => css`
		/* Block container (outer wrapper from UmbTableView) */
		[data-content-type='table'] {
			margin: 1.5rem 0;
		}

		/* Table wrapper - needs position relative for grip positioning */
		.tableWrapper {
			position: relative;
		}

		/* Inner table container */
		.table-container {
			/* Contains the actual table */
		}

		/* Widget container for grips and popovers - positioned to include grip areas */
		.table-controls {
			position: absolute;
			top: -1rem;
			left: -1rem;
			right: -1rem;
			bottom: -1rem;
			pointer-events: none; /* Allow clicks to pass through to table */
			z-index: 10;
		}

		/* Selection overlay container (for future use) */
		.table-selection-overlay {
			position: absolute;
			top: 0;
			left: 0;
			right: 0;
			bottom: 0;
			pointer-events: none;
		}

		/* Grips in the controls container (button elements) */
		.table-controls .grip-column,
		.table-controls .grip-row {
			/* Reset button styles */
			appearance: none;
			border: none;
			padding: 0;
			margin: 0;
			font: inherit;

			position: absolute;
			z-index: 10;
			display: none; /* Hidden by default, shown via JS */
			cursor: pointer;
			align-items: center;
			justify-content: center;
			background-color: rgba(0, 0, 0, 0.05);
			border-color: rgba(0, 0, 0, 0.2);
			pointer-events: auto; /* Grips are clickable */

			uui-symbol-more {
				visibility: hidden;
			}

			&:hover {
				background-color: rgba(0, 0, 0, 0.1);
			}

			&.selected {
				border-color: rgba(0, 0, 0, 0.3);
				background-color: rgba(0, 0, 0, 0.3);
				box-shadow:
					0 0 #0000,
					0 0 #0000,
					0 0 rgba(0, 0, 0, 0.05);
			}

			&:hover uui-symbol-more,
			&.selected uui-symbol-more {
				visibility: visible;
			}
		}

		.table-controls .grip-column {
			border-left-width: 1px;
			/* Container is offset by -1rem, so 0.25rem here = -0.75rem relative to table */
			top: 0.25rem;
			height: 0.75rem;
			/* left and width are set dynamically via JS */
		}

		.table-controls .grip-row {
			border-top-width: 1px;
			flex-direction: column;
			/* Container is offset by -1rem, so 0.25rem here = -0.75rem relative to table */
			left: 0.25rem;
			width: 0.75rem;
			/* top and height are set dynamically via JS */

			uui-symbol-more {
				transform: rotate(90deg);
			}
		}

		/* Popover menu in controls container */
		.table-controls uui-popover-container {
			pointer-events: auto;
		}

		/* Table styles */
		.tableWrapper table {
			border-color: rgba(0, 0, 0, 0.1);
			border-radius: 0.25rem;
			border-spacing: 0;
			box-sizing: border-box;
			max-width: 100%;

			td,
			th {
				box-sizing: border-box;
				position: relative;
				min-width: 50px;
				border: 1px solid var(--uui-color-border);
				padding: 0.5rem;
				text-align: left;
				vertical-align: top;

				&:first-of-type:not(a),
				&:first-of-type:not(a) {
					margin-top: 0;
				}

				p {
					margin: 0;
				}

				p + p {
					margin-top: 0.75rem;
				}
			}

			th {
				font-weight: bold;
			}

			.column-resize-handle {
				cursor: ew-resize;
				cursor: col-resize;
				display: flex;
				position: absolute;
				top: 0;
				bottom: -2px;
				right: -0.25rem;
				width: 0.5rem;
			}

			.column-resize-handle:before {
				margin-left: 0.5rem;
				height: 100%;
				width: 1px;
			}

			.column-resize-handle:before {
				content: '';
			}

			.selectedCell {
				background-color: color-mix(in srgb, var(--uui-color-surface-emphasis) 50%, transparent);
				border-color: var(--uui-color-selected);
			}
		}
	`;
}
