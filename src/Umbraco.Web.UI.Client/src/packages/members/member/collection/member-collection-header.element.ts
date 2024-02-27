import { UmbMemberTypeTreeRepository } from '../../member-type/tree/member-type-tree.repository.js';
import type { UmbMemberTypeItemModel } from '../../member-type/repository/item/types.js';
import type { UmbMemberCollectionContext } from './member-collection.context.js';
import { css, customElement, html, ifDefined, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_DEFAULT_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';
import type { UUIBooleanInputEvent, UUICheckboxElement } from '@umbraco-cms/backoffice/external/uui';

// import './action/create-member-collection-action.element.js';

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
		if (!this._selectedContentTypeUnique) return this.localize.term('general_all');

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
				label=${this.localize.term('visuallyHiddenTexts_userSearchLabel')}
				placeholder=${this.localize.term('visuallyHiddenTexts_userSearchLabel')}
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
						compact
						@click=${() => this.#onContentTypeFilterChange('')}></uui-button>
					${repeat(
						this._contentTypes,
						(memberType) => memberType.unique,
						(memberType) => html`
							<uui-button
								label=${ifDefined(memberType.name)}
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
			#dropdown-layout {
				display: flex;
				flex-direction: column;
				--uui-button-content-align: left;
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
