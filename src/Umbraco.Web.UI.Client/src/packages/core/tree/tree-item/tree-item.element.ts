import type { ManifestTreeItem } from '../extensions/types.js';
import UmbDefaultTreeItemContext from './tree-item-default/tree-item-default.context.js';
import type UmbDefaultTreeItemElement from './tree-item-default/tree-item-default.element.js';
import { customElement, html, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbExtensionElementAndApiSlotElementBase } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-tree-item')
export class UmbTreeItemElement extends UmbExtensionElementAndApiSlotElementBase<ManifestTreeItem> {
	@property({ type: String, reflect: true })
	entityType?: string;

	override render() {
		const entityType = this.entityType;
		if (!entityType) return html``;

		return html`<umb-extension-with-api-slot
			type=${this.getExtensionType()}
			.filter=${(manifest: ManifestTreeItem) => manifest.forEntityTypes.includes(entityType)}
			.elementProps=${this.props}
			.fallbackRenderMethod=${() => this.#renderFallbackResultItem()}></umb-extension-with-api-slot>`;
	}

	#renderFallbackResultItem() {
		const element = document.createElement(this.getDefaultElementName()) as UmbDefaultTreeItemElement;
		const props = this.props;

		// TODO: we could optimize this so we only re-set the updated props.
		if (props) {
			Object.keys(props).forEach((key) => {
				(element as any)[key] = props[key];
			});
		}

		const api = new UmbDefaultTreeItemContext(element);
		element.api = api;

		return element;
	}

	getExtensionType() {
		return 'treeItem';
	}

	getDefaultElementName() {
		return 'umb-default-tree-item';
	}

	override getDefaultApiConstructor() {
		return UmbDefaultTreeItemContext;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-item': UmbTreeItemElement;
	}
}
