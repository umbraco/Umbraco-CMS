import { UMB_USER_COLLECTION_CONTEXT } from './user-collection.context-token.js';
import type { UmbUserOrderByOption } from './types.js';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-user-collection-header')
export class UmbUserCollectionHeaderElement extends UmbLitElement {
	@state()
	private _orderByOptions: Array<UmbUserOrderByOption> = [];

	@state()
	private _activeOrderByOption?: UmbUserOrderByOption;

	#collectionContext?: typeof UMB_USER_COLLECTION_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_USER_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
			this.#observeOrderByOptions();
		});
	}

	#observeOrderByOptions() {
		if (!this.#collectionContext) return;
		this.observe(
			observeMultiple([this.#collectionContext.orderByOptions, this.#collectionContext.activeOrderByOption]),
			([options, activeOption]) => {
				// the options are hardcoded in the context, so we can just compare the length
				if (this._orderByOptions.length !== options.length) {
					this._orderByOptions = options;
				}

				if (activeOption && activeOption !== this._activeOrderByOption?.unique) {
					this._activeOrderByOption = this._orderByOptions.find((option) => option.unique === activeOption);
				}
			},
			'_umbObserveUserOrderByOptions',
		);
	}

	#onOrderByChange(option: UmbUserOrderByOption) {
		this.#collectionContext?.setActiveOrderByOption(option.unique);
	}

	override render() {
		return html`
			<umb-collection-toolbar slot="header">
				<div id="toolbar">
					<umb-collection-filter-field></umb-collection-filter-field>
					${this.#renderOrderBy()}
				</div>
			</umb-collection-toolbar>
		`;
	}

	#renderOrderBy() {
		return html`
			<uui-button popovertarget="popover-order-by-filter" label="order by" compact>
				<umb-localize key="general_orderBy"></umb-localize>:
				<b> ${this._activeOrderByOption ? this.localize.string(this._activeOrderByOption.label) : ''}</b>
			</uui-button>
			<uui-popover-container id="popover-order-by-filter" placement="bottom">
				<umb-popover-layout>
					<div class="filter-dropdown">
						${this._orderByOptions.map(
							(option) => html`
								<uui-menu-item
									label=${this.localize.string(option.label)}
									@click-label=${() => this.#onOrderByChange(option)}
									?active=${this._activeOrderByOption?.unique === option.unique}></uui-menu-item>
							`,
						)}
					</div>
				</umb-popover-layout>
			</uui-popover-container>
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

			#toolbar {
				flex: 1;
				display: flex;
				gap: var(--uui-size-space-4);
				justify-content: space-between;
				align-items: center;
			}

			umb-collection-filter-field {
				width: 100%;
			}

			.filter {
				max-width: 200px;
			}

			.filter-dropdown {
				display: flex;
				gap: var(--uui-size-space-3);
				flex-direction: column;
				padding: var(--uui-size-space-3);
			}
		`,
	];
}

export default UmbUserCollectionHeaderElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-collection-header': UmbUserCollectionHeaderElement;
	}
}
