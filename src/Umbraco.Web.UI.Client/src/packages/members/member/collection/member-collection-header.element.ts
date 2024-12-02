import { UmbMemberTypeTreeRepository } from '../../member-type/tree/member-type-tree.repository.js';
import type { UmbMemberTypeItemModel } from '../../member-type/repository/item/types.js';
import { UMB_MEMBER_COLLECTION_CONTEXT } from './member-collection.context-token.js';
import { css, customElement, html, ifDefined, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-member-collection-header')
export class UmbMemberCollectionHeaderElement extends UmbLitElement {
	#collectionContext?: typeof UMB_MEMBER_COLLECTION_CONTEXT.TYPE;

	// TODO: Should we make a collection repository for member types?
	#contentTypeRepository = new UmbMemberTypeTreeRepository(this);

	@state()
	private _contentTypes: Array<UmbMemberTypeItemModel> = [];

	@state()
	private _selectedContentTypeUnique?: string;

	constructor() {
		super();

		this.consumeContext(UMB_MEMBER_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
		});

		this.#requestContentTypes();
	}

	async #requestContentTypes() {
		const { data } = await this.#contentTypeRepository.requestTreeRootItems({});

		if (data) {
			this._contentTypes = data.items.map((item) => ({
				unique: item.unique,
				name: item.name,
				icon: item.icon || '',
				entityType: item.entityType,
			}));
		}
	}

	get #getContentTypeFilterLabel() {
		if (!this._selectedContentTypeUnique) return this.localize.term('general_all') + ' Member types';
		return (
			this._contentTypes.find((type) => type.unique === this._selectedContentTypeUnique)?.name ||
			this.localize.term('general_all')
		);
	}

	#onContentTypeFilterChange(contentTypeUnique: string) {
		this._selectedContentTypeUnique = contentTypeUnique;
		this.#collectionContext?.setMemberTypeFilter(contentTypeUnique);
	}

	override render() {
		return html`
			<umb-collection-action-bundle></umb-collection-action-bundle>
			<umb-collection-filter-field></umb-collection-filter-field>
			${this.#renderContentTypeFilter()}
		`;
	}

	#renderContentTypeFilter() {
		return html`
			<umb-dropdown>
				<span slot="label">${this.#getContentTypeFilterLabel}</span>
				<div id="dropdown-layout">
					<uui-button
						label=${this.localize.term('general_all')}
						look=${!this._selectedContentTypeUnique ? 'secondary' : 'default'}
						compact
						@click=${() => this.#onContentTypeFilterChange('')}></uui-button>
					${repeat(
						this._contentTypes,
						(memberType) => memberType.unique,
						(memberType) => html`
							<uui-button
								label=${ifDefined(memberType.name)}
								look=${memberType.unique === this._selectedContentTypeUnique ? 'secondary' : 'default'}
								compact
								@click=${() => this.#onContentTypeFilterChange(memberType.unique)}></uui-button>
						`,
					)}
				</div>
			</umb-dropdown>
		`;
	}
	static override styles = [
		css`
			:host {
				height: 100%;
				width: 100%;
				display: flex;
				justify-content: space-between;
				white-space: nowrap;
				gap: var(--uui-size-space-5);
				align-items: center;
			}

			#dropdown-layout {
				display: flex;
				flex-direction: column;
				--uui-button-content-align: left;
			}

			umb-collection-filter-field {
				width: 100%;
			}
		`,
	];
}

export default UmbMemberCollectionHeaderElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-collection-header': UmbMemberCollectionHeaderElement;
	}
}
