import type { UmbDocumentItemModel } from '../../item/types.js';
import { UmbDocumentItemDataResolver } from '../../item/document-item-data-resolver.js';
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
	#resolvers: Array<UmbDocumentItemDataResolver<UmbDocumentItemModel>> = [];

	/**
	 * The ancestors of the document, ordered from the root down.
	 * @type {Array<UmbDocumentItemModel>}
	 */
	@property({ type: Array, attribute: false })
	public set ancestors(value: Array<UmbDocumentItemModel> | undefined) {
		this.#ancestors = value;
		this.#setAncestors(value ?? []);
	}
	public get ancestors(): Array<UmbDocumentItemModel> | undefined {
		return this.#ancestors;
	}
	#ancestors?: Array<UmbDocumentItemModel>;

	@state()
	private _path = '';

	#setAncestors(ancestors: Array<UmbDocumentItemModel>) {
		this.#resolvers.forEach((resolver) => resolver.destroy());
		this.#resolvers = ancestors.map((ancestor) => {
			const resolver = new UmbDocumentItemDataResolver<UmbDocumentItemModel>(this);
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
		return this._path ? html`${this._path}` : nothing;
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
