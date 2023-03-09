import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement, nothing } from 'lit';
import { repeat } from 'lit-html/directives/repeat.js';
import { customElement, query, state } from 'lit/decorators.js';

export type SearchItem = {
	name: string;
	icon?: string;
	href: string;
	parent: string;
	url?: string;
};
export type SearchGroupItem = {
	name: string;
	items: Array<SearchItem>;
};
@customElement('umb-search-modal')
export class UmbSearchModalElement extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				height: 100%;
				width: 100%;
				height: 100%;
				background-color: var(--uui-color-background);
				box-sizing: border-box;
				color: var(--uui-color-text);
				font-size: 1rem;
			}
			input {
				all: unset;
				height: 100%;
				width: 100%;
			}
			#search-icon,
			#close-icon {
				display: flex;
				align-items: center;
				justify-content: center;
				aspect-ratio: 1;
				height: 100%;
			}
			#close-icon {
				padding: 0 var(--uui-size-space-4);
			}
			#close-icon > button {
				background: var(--uui-color-surface-alt);
				border: 1px solid var(--uui-color-border);
				padding: 3px 6px 4px 6px;
				line-height: 1;
				border-radius: 3px;
				color: var(--uui-color-text-alt);
				font-weight: 800;
				font-size: 12px;
				cursor: pointer;
			}
			#close-icon > button:hover {
				border-color: var(--uui-color-focus);
				color: var(--uui-color-focus);
			}
			#top {
				background-color: var(--uui-color-surface);
				display: flex;
				height: 48px;
			}
			#main {
				display: flex;
				flex-direction: column;
				padding: 0px var(--uui-size-space-6) var(--uui-size-space-5) var(--uui-size-space-6);
				height: 100%;
				border-top: 1px solid var(--uui-color-border);
			}
			.group {
				margin-top: var(--uui-size-space-4);
			}
			.group-name {
				font-weight: 600;
				margin-bottom: var(--uui-size-space-1);
			}
			.group-items {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-3);
			}
			.item {
				background: var(--uui-color-surface);
				border: 1px solid var(--uui-color-border);
				padding: var(--uui-size-space-3) var(--uui-size-space-4);
				border-radius: var(--uui-border-radius);
				color: var(--uui-color-interactive);
				display: grid;
				grid-template-columns: var(--uui-size-space-6) 1fr var(--uui-size-space-5);
				height: min-content;
				align-items: center;
			}
			.item:hover {
				background-color: var(--uui-color-surface-emphasis);
				color: var(--uui-color-interactive-emphasis);
			}
			.item:hover .item-symbol {
				font-weight: unset;
				opacity: 1;
			}
			.item-icon {
				margin-bottom: auto;
				margin-top: 5px;
			}
			.item-icon,
			.item-symbol {
				opacity: 0.4;
			}
			.item-url {
				font-size: 0.8rem;
				line-height: 1.2;
				font-weight: 100;
			}
			.item-name {
				display: flex;
				flex-direction: column;
			}
			.item-icon > * {
				height: 1rem;
				display: flex;
				width: min-content;
			}
			.item-symbol {
				font-weight: 100;
			}
			a {
				text-decoration: none;
				color: inherit;
			}
			#no-results {
				display: flex;
				flex-direction: column;
				align-items: center;
				justify-content: center;
				height: 100%;
				width: 100%;
				margin-top: var(--uui-size-space-5);
				color: var(--uui-color-text-alt);
			}
		`,
	];

	@query('input')
	private _input!: HTMLInputElement;

	@state()
	private _search = '';

	@state()
	private _groups: Array<SearchGroupItem> = [];

	connectedCallback() {
		super.connectedCallback();

		requestAnimationFrame(() => {
			this._input.focus();
		});
	}

	#onSearchChange(event: InputEvent) {
		const target = event.target as HTMLInputElement;
		this._search = target.value;

		this.#updateGroups();
	}

	#onClearSearch() {
		this._search = '';
		this._input.value = '';
		this._input.focus();
		this.#updateGroups();
	}

	#updateGroups() {
		const filtered = this.#mockData.filter((item) => {
			return item.name.toLowerCase().includes(this._search.toLowerCase());
		});

		const grouped: Array<SearchGroupItem> = filtered.reduce((acc, item) => {
			const group = acc.find((group) => group.name === item.parent);
			if (group) {
				group.items.push(item);
			} else {
				acc.push({
					name: item.parent,
					items: [item],
				});
			}
			return acc;
		}, [] as Array<SearchGroupItem>);

		this._groups = grouped;
	}

	render() {
		return html`
			<div id="top">
				<div id="search-icon">
					<uui-icon name="search"></uui-icon>
				</div>
				<input
					value=${this._search}
					@input=${this.#onSearchChange}
					type="text"
					placeholder="Search..."
					autocomplete="off" />
				<div id="close-icon">
					<button @click=${this.#onClearSearch}>clear</button>
				</div>
			</div>
			${this._search
				? html`<div id="main">
						${this._groups.length > 0
							? repeat(
									this._groups,
									(group) => group.name,
									(group) => this.#renderGroup(group.name, group.items)
							  )
							: html`<div id="no-results">Only mock data for now <strong>Search for blog</strong></div>`}
				  </div>`
				: nothing}
		`;
	}

	#renderGroup(name: string, items: Array<SearchItem>) {
		return html`
			<div class="group">
				<div class="group-name">${name}</div>
				<div class="group-items">${repeat(items, (item) => item.name, this.#renderItem.bind(this))}</div>
			</div>
		`;
	}

	#renderItem(item: SearchItem) {
		return html`
			<a href="${item.href}" class="item">
				<span class="item-icon">
					${item.icon ? html`<uui-icon name="${item.icon}"></uui-icon>` : this.#renderHashTag()}
				</span>
				<span class="item-name">
					${item.name} ${item.url ? html`<span class="item-url">${item.url}</span>` : nothing}
				</span>
				<span class="item-symbol">></span>
			</a>
		`;
	}

	#renderHashTag() {
		return html`
			<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="24" height="24">
				<path fill="none" d="M0 0h24v24H0z" />
				<path
					fill="currentColor"
					d="M7.784 14l.42-4H4V8h4.415l.525-5h2.011l-.525 5h3.989l.525-5h2.011l-.525 5H20v2h-3.784l-.42 4H20v2h-4.415l-.525 5h-2.011l.525-5H9.585l-.525 5H7.049l.525-5H4v-2h3.784zm2.011 0h3.99l.42-4h-3.99l-.42 4z" />
			</svg>
		`;
	}

	#mockData: Array<SearchItem> = [
		{
			name: 'Blog',
			href: '#',
			icon: 'umb:thumbnail-list',
			parent: 'Content',
			url: '/blog/',
		},
		{
			name: 'Popular blogs',
			href: '#',
			icon: 'umb:article',
			parent: 'Content',
			url: '/blog/popular-blogs/',
		},
		{
			name: 'How to write a blog',
			href: '#',
			icon: 'umb:article',
			parent: 'Content',
			url: '/blog/how-to-write-a-blog/',
		},
		{
			name: 'Blog hero',
			href: '#',
			icon: 'umb:picture',
			parent: 'Media',
		},
		{
			name: 'Contact form for blog',
			href: '#',
			parent: 'Document Types',
		},
		{
			name: 'Blog',
			href: '#',
			parent: 'Document Types',
		},
		{
			name: 'Blog link item',
			href: '#',
			parent: 'Document Types',
		},
	];
}

export default UmbSearchModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-search-modal': UmbSearchModalElement;
	}
}
