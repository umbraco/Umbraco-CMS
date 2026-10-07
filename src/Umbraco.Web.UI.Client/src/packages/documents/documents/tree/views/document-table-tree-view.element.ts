import type { UmbDocumentTreeItemModel } from '../types.js';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import type { TemplateResult } from '@umbraco-cms/backoffice/external/lit';
import { UmbTableTreeViewElement } from '@umbraco-cms/backoffice/tree';
import type { UmbTreeItemModel } from '@umbraco-cms/backoffice/tree';

@customElement('umb-document-table-tree-view')
export class UmbDocumentTableTreeViewElement extends UmbTableTreeViewElement {
	// A collection the user cannot access stays expandable, so the way down to their start node remains reachable.
	#hasCollection(item: UmbTreeItemModel): boolean {
		const document = item as UmbDocumentTreeItemModel;

		// TODO (V20): drop the `documentType` fallback when the deprecated field is removed.
		// eslint-disable-next-line @typescript-eslint/no-deprecated
		return !!(document.contentType ?? document.documentType)?.collection && !document.noAccess;
	}

	protected override _showExpandSymbol(item: UmbTreeItemModel): boolean {
		return this.#hasCollection(item) || super._showExpandSymbol(item);
	}

	protected override _getExpandPath(item: UmbTreeItemModel, path: string | undefined): string | undefined {
		return path && this.#hasCollection(item) ? `${path}?openCollection=true` : path;
	}

	protected override _renderExpandSymbol(item: UmbTreeItemModel): TemplateResult | undefined {
		return this.#hasCollection(item)
			? html`<umb-icon name="icon-list" style="font-size: 8px;"></umb-icon>`
			: super._renderExpandSymbol(item);
	}
}

export { UmbDocumentTableTreeViewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-table-tree-view': UmbDocumentTableTreeViewElement;
	}
}
