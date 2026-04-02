import type { ManifestTreeItem } from '../extensions/types.js';
import UmbDefaultTreeItemContext from './tree-item-default/tree-item-default.context.js';
import { customElement, property } from '@umbraco-cms/backoffice/external/lit';
import {
	UmbExtensionElementAndApiSlotElementBase,
	umbExtensionsRegistry,
} from '@umbraco-cms/backoffice/extension-registry';
import { createObservablePart } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-tree-item')
export class UmbTreeItemElement extends UmbExtensionElementAndApiSlotElementBase<ManifestTreeItem> {
	@property({ type: String, reflect: true })
	get entityType() {
		return this.#entityType;
	}
	set entityType(newVal) {
		this.#entityType = newVal;
		this.#observeEntityType();
	}
	#entityType?: string;

	@property({ type: Object, attribute: false })
	override set props(newVal: Record<string, unknown> | undefined) {
		super.props = newVal;
		this.#assignProps();
	}
	override get props() {
		return super.props;
	}

	#observeEntityType() {
		if (!this.#entityType) return;

		const filterByEntityType = (manifest: ManifestTreeItem) => {
			if (!this.#entityType) return false;
			return manifest.forEntityTypes.includes(this.#entityType);
		};

		// Check if we can find a matching tree item for the current entity type.
		// If we can, we will use that one, if not we will render a fallback tree item.
		this.observe(
			// TODO: what should we do if there are multiple tree items for an entity type?
			// This method gets all extensions based on a type, then filters them based on the entity type. and then we get the alias of the first one [NL]
			createObservablePart(
				umbExtensionsRegistry.byTypeAndFilter(this.getExtensionType(), filterByEntityType),
				(x) => x[0]?.alias,
			),
			(alias) => {
				this.alias = alias;

				// If we don't find any registered tree items for this specific entity type, we will render a fallback tree item.
				// This is on purpose not done with the extension initializer since we don't want to spin up a real extension unless we have to.
				if (!alias) {
					this.#renderFallbackItem();
				}
			},
			'umbObserveAlias',
		);
	}

	#renderFallbackItem() {
		// TODO: make creating of elements with apis a shared function.
		const element = document.createElement('umb-default-tree-item');
		const api = new UmbDefaultTreeItemContext(element);
		element.api = api;
		this._element = element;
		this.#assignProps();
		this.requestUpdate('_element');
	}

	getExtensionType() {
		return 'treeItem';
	}

	getDefaultElementName() {
		return 'umb-default-tree-item';
	}

	#assignProps() {
		if (!this._element || !this.props) return;

		Object.keys(this.props).forEach((key) => {
			(this._element as any)[key] = this.props![key];
		});
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
