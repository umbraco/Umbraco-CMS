import type { UmbDocumentTreeItemModel } from '../../tree/types.js';
import { UmbDocumentTreeItemDataResolver } from '../../tree/document-tree-item-data-resolver.js';
import { css, customElement, html, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { mergeObservables } from '@umbraco-cms/backoffice/observable-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const ANCESTOR_PATH_OBSERVER_ALIAS = 'umbObserveAncestorPath';

/**
 * Renders the names of a document's ancestors as a path, using the names for the current culture.
 * @element umb-document-ancestor-path
 */
@customElement('umb-document-ancestor-path')
export class UmbDocumentAncestorPathElement extends UmbLitElement {
	#resolvers: Array<UmbDocumentTreeItemDataResolver> = [];

	/**
	 * The ancestors of the document, ordered from the root down.
	 * @type {Array<UmbDocumentTreeItemModel>}
	 */
	@property({ type: Array, attribute: false })
	public set ancestors(value: Array<UmbDocumentTreeItemModel> | undefined) {
		this.#ancestors = value;
		this.#setAncestors(value ?? []);
	}
	public get ancestors(): Array<UmbDocumentTreeItemModel> | undefined {
		return this.#ancestors;
	}
	#ancestors?: Array<UmbDocumentTreeItemModel>;

	@state()
	private _path = '';

	#setAncestors(ancestors: Array<UmbDocumentTreeItemModel>) {
		this.#resolvers.forEach((resolver) => resolver.destroy());
		this.#resolvers = ancestors.map((ancestor) => {
			const resolver = new UmbDocumentTreeItemDataResolver(this);
			resolver.setData(ancestor);
			return resolver;
		});

		if (this.#resolvers.length === 0) {
			this.removeUmbControllerByAlias(ANCESTOR_PATH_OBSERVER_ALIAS);
			this._path = '';
			return;
		}

		this.observe(
			mergeObservables(
				this.#resolvers.map((resolver) => resolver.name),
				(names) => (names.every(Boolean) ? names.join(' / ') : ''),
			),
			(path) => (this._path = path),
			ANCESTOR_PATH_OBSERVER_ALIAS,
		);
	}

	override render() {
		// The host is right-to-left so overflow truncates the start of the path; isolating the text keeps its own
		// direction, so punctuation at either end stays in place.
		return this._path ? html`<bdi>${this._path}</bdi>` : nothing;
	}

	static override styles = [
		css`
			:host {
				display: block;
				overflow: hidden;
				text-overflow: ellipsis;
				white-space: nowrap;
				direction: rtl;
				text-align: left;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-ancestor-path': UmbDocumentAncestorPathElement;
	}
}
