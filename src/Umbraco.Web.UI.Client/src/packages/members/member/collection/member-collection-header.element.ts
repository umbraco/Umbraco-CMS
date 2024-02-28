import { UmbMemberTypeTreeRepository } from '../../member-type/tree/member-type-tree.repository.js';
import type { UmbMemberTypeItemModel } from '../../member-type/repository/item/types.js';
import type { UmbMemberCollectionContext } from './member-collection.context.js';
import { css, customElement, html, ifDefined, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_DEFAULT_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';

@customElement('umb-member-collection-header')
export class UmbMemberCollectionHeaderElement extends UmbLitElement {
	@state()
	private _contentTypes: Array<UmbMemberTypeItemModel> = [];

	#inputTimer?: NodeJS.Timeout;
	#inputTimerAmount = 300;

	#collectionContext?: UmbMemberCollectionContext;
	// TODO: Should we make a collection repository for member types?
	#contentTypeRepository = new UmbMemberTypeTreeRepository(this);

	@state()
	private _selectedContentTypeUnique?: string;

	constructor() {
		super();

		this.consumeContext(UMB_DEFAULT_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance as UmbMemberCollectionContext;
		});

		this.#requestContentTypes();
	}

	async #requestContentTypes() {
		const { data } = await this.#contentTypeRepository.requestRootTreeItems();

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

	#onSearch(event: InputEvent) {
		const target = event.target as HTMLInputElement;
		const filter = target.value || '';
		clearTimeout(this.#inputTimer);
		this.#inputTimer = setTimeout(() => this.#collectionContext?.setFilter({ filter }), this.#inputTimerAmount);
	}

	#onContentTypeFilterChange(contentTypeUnique: string) {
		this._selectedContentTypeUnique = contentTypeUnique;
		this.#collectionContext?.setMemberTypeFilter(contentTypeUnique);
	}

	render() {
		return html`<umb-collection-action-bundle></umb-collection-action-bundle>
			<uui-input
				@input=${this.#onSearch}
				label=${this.localize.term('general_search')}
				placeholder=${this.localize.term('general_search')}
				id="input-search"></uui-input>
			${this.#renderContentTypeFilter()} `;
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
	static styles = [
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

			uui-input {
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
