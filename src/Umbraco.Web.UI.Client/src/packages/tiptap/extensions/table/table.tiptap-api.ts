import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { css } from '@umbraco-cms/backoffice/external/lit';
import { UmbTable, UmbTableHeader, UmbTableRow, UmbTableCell } from '@umbraco-cms/backoffice/external/tiptap';

import '../../components/menu/tiptap-menu.element.js';

export default class UmbTiptapTableExtensionApi extends UmbTiptapExtensionApiBase {
	// eslint-disable-next-line @typescript-eslint/no-deprecated
	getTiptapExtensions = () => [UmbTable, UmbTableHeader, UmbTableRow, UmbTableCell];

	override getStyles = () => css`
		.tableWrapper {
			margin: 1.5rem 0;

			table {
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

				.grip-column,
				.grip-row {
					position: absolute;
					z-index: 10;
					display: flex;
					cursor: pointer;
					align-items: center;
					justify-content: center;
					background-color: rgba(0, 0, 0, 0.05);
					border-color: rgba(0, 0, 0, 0.2);

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

				.grip-column {
					border-left-width: 1px;
					top: -0.75rem;
					left: 0;
					height: 0.75rem;
					width: calc(100% + 1px);
					margin-left: -1px;
				}

				.grip-row {
					border-top-width: 1px;
					flex-direction: column;
					top: 0;
					left: -0.75rem;
					height: calc(100% + 1px);
					width: 0.75rem;
					margin-top: -1px;

					uui-symbol-more {
						transform: rotate(90deg);
					}
				}
			}
		}
	`;
}
