/* eslint-disable local-rules/enforce-umbraco-external-imports */
import type { ProseMirrorNode } from '../../../externals.js';
import { TableView } from '@tiptap/pm/tables';
import type { ViewMutationRecord } from '@tiptap/pm/view';

export class UmbTableView extends TableView {
	#containerAttributes: Record<string, string>;
	#blockContainer: HTMLDivElement;
	#innerTableContainer: HTMLDivElement;
	#controlsContainer: HTMLDivElement;

	constructor(node: ProseMirrorNode, cellMinWidth: number, containerAttributes: Record<string, string>) {
		super(node, cellMinWidth);
		this.#containerAttributes = containerAttributes ?? {};
		this.#blockContainer = this.#createBlockContainer();
		this.#innerTableContainer = this.#createInnerTableContainer();
		this.#controlsContainer = this.#createControlsContainer();
		this.#setupDOMStructure();
		this.#updateTableStyle(node);
	}

	#createBlockContainer(): HTMLDivElement {
		const container = document.createElement('div');
		container.className = 'umb-tiptap-table';

		Object.entries(this.#containerAttributes).forEach(([key, value]) => {
			if (key === 'class') {
				container.className += ' ' + value;
			} else {
				container.setAttribute(key, value);
			}
		});

		return container;
	}

	#createInnerTableContainer(): HTMLDivElement {
		const container = document.createElement('div');
		container.className = 'table-container';
		return container;
	}

	#createControlsContainer(): HTMLDivElement {
		const container = document.createElement('div');
		container.className = 'table-controls';
		return container;
	}

	#setupDOMStructure(): void {
		// The parent TableView creates: <div class="tableWrapper"><table>...</table></div>
		// We restructure to: blockContainer > tableWrapper > innerTableContainer + controlsContainer
		const tableWrapper = this.dom as HTMLElement;
		const tableElement = tableWrapper.firstChild;
		if (!tableElement) return;

		// Move table into inner container
		this.#innerTableContainer.appendChild(tableElement);

		// Build the new structure
		tableWrapper.appendChild(this.#innerTableContainer);
		tableWrapper.appendChild(this.#controlsContainer);
		this.#blockContainer.appendChild(tableWrapper);

		// Update the DOM reference
		this.dom = this.#blockContainer;
	}

	override ignoreMutation(mutation: ViewMutationRecord): boolean {
		// Only track mutations inside the table-container (the actual table content)
		// Ignore mutations in controls and overlay containers
		const target = mutation.target as HTMLElement;
		const isInsideTable = target.closest('.table-container');
		return !isInsideTable || super.ignoreMutation(mutation);
	}

	override update(node: ProseMirrorNode): boolean {
		if (!super.update(node)) return false;
		this.#updateTableStyle(node);
		return true;
	}

	#updateTableStyle(node: ProseMirrorNode) {
		if (node.attrs.style) {
			// NOTE: The `min-width` inline style is handled by the Tiptap TableView, so we need to preserve it. [LK]
			const minWidth = this.table.style.minWidth;
			const styles = node.attrs.style as string;
			this.table.style.cssText = `${styles}; min-width: ${minWidth};`;
		}
	}
}
