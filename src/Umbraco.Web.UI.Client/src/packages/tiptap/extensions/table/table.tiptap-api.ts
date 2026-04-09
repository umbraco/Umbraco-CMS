import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { UmbTable, UmbTableHeader, UmbTableRow, UmbTableCell } from './table.tiptap-extension.js';
import { css } from '@umbraco-cms/backoffice/external/lit';

import '../../components/menu/tiptap-menu.element.js';

export default class UmbTiptapTableExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [UmbTable, UmbTableHeader, UmbTableRow, UmbTableCell];

	override getStyles = () => css`
		/* Block container (outer wrapper from UmbTableView) */
		.umb-tiptap-table {
			margin-block: 1.25rem;

			/* Table wrapper - needs position relative for grip positioning */
			.tableWrapper {
				position: relative;
			}

			/* Inner table container */
			.table-container {
				max-width: 100%;

				/* Table styles */
				table {
					border-color: rgba(0, 0, 0, 0.1);
					border-radius: 0.25rem;
					border-spacing: 0;
					box-sizing: border-box;
					max-width: 100%;
					overflow-wrap: break-word;

					td,
					th {
						box-sizing: border-box;
						position: relative;
						min-width: 50px;
						border: 1px solid var(--uui-color-border);
						padding: 0.5rem;
						text-align: left;
						vertical-align: top;

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
						background-color: var(--uui-color-surface-emphasis);
						font-weight: bold;
					}

					.column-resize-handle {
						position: absolute;
						top: 0;
						right: 0;
						width: 4px;
						height: 100%;
						margin-inline-start: -1px;
						margin-top: -1px;
						height: calc(100% + 2px);
						background: var(--uui-color-divider-emphasis);
						cursor: col-resize;
						z-index: 1;
						pointer-events: auto;
					}

					.selectedCell {
						background-color: color-mix(in srgb, var(--uui-color-surface-emphasis) 50%, transparent);
						border-color: var(--uui-color-selected);
					}
				}
			}

			/* Controls container for grips and popovers - positioned to include grip areas */
			.table-controls {
				position: absolute;
				top: -1rem;
				left: -1rem;
				right: -1rem;
				bottom: -1rem;
				pointer-events: none; /* Allow clicks to pass through to table */
				z-index: 10;

				/* Grips in the controls container (button elements) */
				.grip-column,
				.grip-row {
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
				}

				.grip-column {
					border-left-width: 1px;
					/* Container is offset by -1rem, so 0.25rem here = -0.75rem relative to table */
					top: 0.25rem;
					height: 0.75rem;
					/* left and width are set dynamically via JS */
				}

				.grip-row {
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
				uui-popover-container {
					pointer-events: auto;
				}
			}
		}
	`;
}
