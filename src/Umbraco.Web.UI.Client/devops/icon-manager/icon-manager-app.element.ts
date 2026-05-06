import { LitElement, html, css, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { iconNodesToSvg } from './svg-utils.js';
import type { IconDictionary, IconCategory, IconNodes, IconTags, ManagedIcon } from './types.js';
import './icon-card.element.js';
import './icon-editor.element.js';
import '@umbraco-ui/uui';

// Static imports — Vite resolves JSON natively
import rawDictionary from '../../src/packages/core/icon-registry/icon-dictionary.json';
import rawIconNodes from 'lucide-static/icon-nodes.json';
import rawIconTags from 'lucide-static/tags.json';

const iconDictionary = rawDictionary as unknown as IconDictionary;
const iconNodes = rawIconNodes as unknown as IconNodes;
const iconTags = rawIconTags as unknown as IconTags;

@customElement('icon-manager-app')
export class IconManagerAppElement extends LitElement {
	@state()
	private _pickedIcons: ManagedIcon[] = [];

	@state()
	private _unpickedIcons: ManagedIcon[] = [];

	@state()
	private _activeTab: 'picked' | 'unpicked' = 'picked';

	@state()
	private _searchQuery = '';

	@state()
	private _editingIcon: ManagedIcon | null = null;

	@state()
	private _editingIndex = -1;

	@state()
	private _allGroups: string[] = [];

	#searchTimeout: ReturnType<typeof setTimeout> | undefined;

	override connectedCallback() {
		super.connectedCallback();
		this.#loadData();
		this.addEventListener('icon-select', this.#onIconSelect as EventListener);
		this.addEventListener('icon-add', this.#onIconAdd as EventListener);
		this.addEventListener('icon-updated', this.#onIconUpdated as EventListener);
		this.addEventListener('editor-close', this.#onEditorClose as EventListener);
	}

	#loadData() {
		const picked: ManagedIcon[] = [];
		const pickedLucideFiles = new Set<string>();

		// Process each category from the dictionary
		const categories: IconCategory[] = ['lucide', 'simpleIcons', 'umbraco', 'custom'];
		for (const category of categories) {
			const entries = iconDictionary[category] ?? [];
			for (const entry of entries) {
				if (!entry.name || !entry.file) continue;
				const lucideName = entry.file.replace('.svg', '');

				if (category === 'lucide') {
					pickedLucideFiles.add(lucideName);
				}

				const nodes = category === 'lucide' ? iconNodes[lucideName] : undefined;
				const svgMarkup = nodes ? iconNodesToSvg(nodes) : '';
				const tags = category === 'lucide' ? (iconTags[lucideName] ?? []) : [];

				picked.push({
					name: entry.name,
					file: entry.file,
					category,
					keywords: entry.keywords ?? [],
					groups: entry.groups ?? [],
					related: entry.related ?? [],
					legacy: entry.legacy ?? false,
					internal: entry.internal ?? false,
					svgMarkup,
					lucideTags: tags,
					isNew: false,
					isDirty: false,
				});
			}
		}

		// Build unpicked Lucide icons
		const unpicked: ManagedIcon[] = [];
		for (const [lucideName, nodes] of Object.entries(iconNodes)) {
			if (!pickedLucideFiles.has(lucideName)) {
				unpicked.push({
					name: `icon-${lucideName}`,
					file: `${lucideName}.svg`,
					category: 'lucide',
					keywords: [],
					groups: [],
					related: [],
					legacy: false,
					internal: false,
					svgMarkup: iconNodesToSvg(nodes),
					lucideTags: iconTags[lucideName] ?? [],
					isNew: false,
					isDirty: false,
				});
			}
		}

		this._pickedIcons = picked;
		this._unpickedIcons = unpicked;
		this.#updateGroups();
	}

	#updateGroups() {
		const groups = new Set<string>();
		for (const icon of this._pickedIcons) {
			for (const g of icon.groups) {
				groups.add(g);
			}
		}
		this._allGroups = [...groups].sort();
	}

	get #filteredIcons(): ManagedIcon[] {
		const icons = this._activeTab === 'picked' ? this._pickedIcons : this._unpickedIcons;
		if (!this._searchQuery) return icons;

		const q = this._searchQuery.toLowerCase();
		return icons.filter((icon) => {
			if (icon.name.toLowerCase().includes(q)) return true;
			if (icon.file.toLowerCase().includes(q)) return true;
			if (icon.keywords.some((k) => k.toLowerCase().includes(q))) return true;
			if (icon.groups.some((g) => g.toLowerCase().includes(q))) return true;
			if (icon.lucideTags.some((t) => t.toLowerCase().includes(q))) return true;
			return false;
		});
	}

	get #changeCount(): number {
		return this._pickedIcons.filter((i) => i.isNew || i.isDirty).length;
	}

	#onSearchInput(e: Event) {
		const input = e.target as HTMLInputElement;
		clearTimeout(this.#searchTimeout);
		this.#searchTimeout = setTimeout(() => {
			this._searchQuery = input.value;
		}, 250);
	}

	#onIconSelect = (e: Event) => {
		const detail = (e as CustomEvent<ManagedIcon>).detail;
		this._editingIcon = detail;
		this._editingIndex = this._pickedIcons.indexOf(detail);
	};

	#onIconAdd = (e: Event) => {
		const icon = (e as CustomEvent<ManagedIcon>).detail;
		// Move from unpicked to picked
		const newIcon: ManagedIcon = {
			...icon,
			keywords: [...(iconTags[icon.file.replace('.svg', '')] ?? [])],
			isNew: true,
		};
		const newPicked = [...this._pickedIcons, newIcon];
		this._pickedIcons = newPicked;
		this._unpickedIcons = this._unpickedIcons.filter((i) => i.file !== icon.file);
		this.#updateGroups();
		// Open editor for the newly added icon
		this._editingIcon = newIcon;
		this._editingIndex = newPicked.length - 1;
	};

	#onIconUpdated = (e: Event) => {
		const updated = (e as CustomEvent<ManagedIcon>).detail;
		if (this._editingIndex >= 0) {
			const copy = [...this._pickedIcons];
			copy[this._editingIndex] = updated;
			this._pickedIcons = copy;
		}
		this._editingIcon = updated;
		this.#updateGroups();
	};

	#onEditorClose = () => {
		this._editingIcon = null;
		this._editingIndex = -1;
	};

	#exportJson() {
		const dictionary: IconDictionary = { lucide: [], simpleIcons: [], umbraco: [], custom: [] };

		for (const icon of this._pickedIcons) {
			const entry: Record<string, unknown> = { name: icon.name, file: icon.file };
			if (icon.keywords.length) entry.keywords = icon.keywords;
			if (icon.groups.length) entry.groups = icon.groups;
			if (icon.related.length) entry.related = icon.related;
			if (icon.legacy) entry.legacy = true;
			if (icon.internal) entry.internal = true;
			(dictionary[icon.category] as Record<string, unknown>[]).push(entry);
		}

		const json = JSON.stringify(dictionary, null, '\t');
		const blob = new Blob([json + '\n'], { type: 'application/json' });
		const url = URL.createObjectURL(blob);
		const a = document.createElement('a');
		a.href = url;
		a.download = 'icon-dictionary.json';
		a.click();
		URL.revokeObjectURL(url);
	}

	override render() {
		const filtered = this.#filteredIcons;
		const pickedCount = this._pickedIcons.length;
		const unpickedCount = this._unpickedIcons.length;

		return html`
			<div class="layout ${this._editingIcon ? 'with-editor' : ''}">
				<header class="toolbar">
					<h1>Icon Dictionary Manager</h1>
					<div class="tabs">
						<uui-button
							label="Show picked icons"
							look=${this._activeTab === 'picked' ? 'primary' : 'secondary'}
							@click=${() => (this._activeTab = 'picked')}>
							Picked (${pickedCount})
						</uui-button>
						<uui-button
							label="Show unpicked icons"
							look=${this._activeTab === 'unpicked' ? 'primary' : 'secondary'}
							@click=${() => (this._activeTab = 'unpicked')}>
							Unpicked (${unpickedCount})
						</uui-button>
					</div>
					<uui-input
						class="search"
						label="Search icons"
						placeholder="Search icons..."
						@input=${this.#onSearchInput}
						type="search"></uui-input>
					<uui-button label="Export JSON" look="primary" @click=${this.#exportJson}>
						Export JSON ${this.#changeCount > 0 ? `(${this.#changeCount} changes)` : ''}
					</uui-button>
				</header>

				<main class="grid-area">
					<div class="icon-count">${filtered.length} icons</div>
					<div class="icon-grid">
						${repeat(
							filtered,
							(icon) => `${icon.category}-${icon.file}-${icon.name}`,
							(icon) => html`<icon-card .icon=${icon} .mode=${this._activeTab}></icon-card>`,
						)}
					</div>
				</main>

				${this._editingIcon
					? html`
							<aside class="editor-panel">
								<icon-editor
									.icon=${this._editingIcon}
									.allIcons=${this._pickedIcons}
									.allGroups=${this._allGroups}></icon-editor>
							</aside>
						`
					: nothing}
			</div>
		`;
	}

	static override styles = css`
		:host {
			display: block;
			height: 100vh;
		}

		.layout {
			display: grid;
			grid-template-rows: auto 1fr;
			grid-template-columns: 1fr;
			height: 100%;
		}

		.layout.with-editor {
			grid-template-columns: 1fr 360px;
		}

		.toolbar {
			grid-column: 1 / -1;
			display: flex;
			align-items: center;
			gap: 12px;
			padding: 12px 16px;
			background: var(--uui-color-surface, #fff);
			border-bottom: 1px solid var(--uui-color-border, #d8d7d9);
			flex-wrap: wrap;
		}

		.toolbar h1 {
			font-size: 16px;
			margin: 0;
			white-space: nowrap;
		}

		.tabs {
			display: flex;
			gap: 4px;
		}

		.search {
			flex: 1;
			min-width: 200px;
		}

		.grid-area {
			overflow-y: auto;
			padding: 16px;
		}

		.icon-count {
			font-size: 12px;
			opacity: 0.5;
			margin-bottom: 8px;
		}

		.icon-grid {
			display: grid;
			grid-template-columns: repeat(auto-fill, minmax(110px, 1fr));
			gap: 8px;
		}

		.editor-panel {
			background: var(--uui-color-surface, #fff);
			border-left: 1px solid var(--uui-color-border, #d8d7d9);
			overflow-y: auto;
		}
	`;
}

declare global {
	interface HTMLElementTagNameMap {
		'icon-manager-app': IconManagerAppElement;
	}
}
