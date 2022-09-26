import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { Subscription } from 'rxjs';
import { UmbContextConsumerMixin } from '../../../../context';

import type { UUIInputEvent } from '@umbraco-ui/uui';
import type { UmbModalHandler } from '../../modal-handler';
import type { UmbExtensionRegistry } from '../../../../extension';
import type { ManifestPropertyEditorUI } from '../../../../models';

export interface UmbModalPropertyEditorUIPickerData {
	selection?: Array<string>;
	submitLabel?: string;
}

interface GroupedPropertyEditorUIs {
	[key: string]: Array<ManifestPropertyEditorUI>;
}

const groupBy = (xs: Array<any>, key: string) => {
	return xs.reduce(function (rv, x) {
		(rv[x[key]] = rv[x[key]] || []).push(x);
		return rv;
	}, {});
};

@customElement('umb-modal-layout-property-editor-ui-picker')
export class UmbModalLayoutPropertyEditorUIPickerElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [
		UUITextStyles,
		css`
			#filter {
				width: 100%;
				margin-bottom: var(--uui-size-space-4);
			}

			#filter-icon {
				padding-left: var(--uui-size-space-2);
			}

			#item-grid {
				display: grid;
				grid-template-columns: repeat(auto-fill, minmax(70px, 1fr));
				margin: 0;
				padding: 0;
				grid-gap: var(--uui-size-space-4);
			}

			#item-grid .item {
				display: flex;
				align-items: flex-start;
				justify-content: center;
				list-style: none;
				height: 100%;
				border: 1px solid transparent;
				border-radius: var(--uui-border-radius);
			}

			#item-grid .item:hover {
				background: var(--uui-color-surface-emphasis);
				color: var(--uui-color-interactive-emphasis);
				cursor: pointer;
			}

			#item-grid .item[selected] button {
				background: var(--uui-color-selected);
				color: var(--uui-color-selected-contrast);
			}

			#item-grid .item button {
				background: none;
				border: none;
				cursor: pointer;
				padding: var(--uui-size-space-3);
				display: flex;
				align-items: center;
				flex-direction: column;
				justify-content: center;
				font-size: 0.8rem;
				height: 100%;
				width: 100%;
				color: var(--uui-color-interactive);
				border-radius: var(--uui-border-radius);
			}

			#item-grid .item .icon {
				font-size: 2em;
				margin-bottom: var(--uui-size-space-2);
			}
		`,
	];

	@property({ attribute: false })
	modalHandler?: UmbModalHandler;

	@property({ type: Object })
	data?: UmbModalPropertyEditorUIPickerData;

	@state()
	private _groupedPropertyEditorUIs: GroupedPropertyEditorUIs = {};

	@state()
	private _propertyEditorUIs: Array<ManifestPropertyEditorUI> = [];

	@state()
	private _selection: Array<string> = [];

	@state()
	private _submitLabel = 'Select';

	private _extensionRegistry?: UmbExtensionRegistry;
	private _propertyEditorUIsSubscription?: Subscription;

	constructor() {
		super();

		this.consumeContext('umbExtensionRegistry', (registry) => {
			this._extensionRegistry = registry;
			this._usePropertyEditorUIs();
		});
	}

	connectedCallback(): void {
		super.connectedCallback();

		this._selection = this.data?.selection ?? [];
		this._submitLabel = this.data?.submitLabel ?? this._submitLabel;
	}

	private _usePropertyEditorUIs() {
		if (!this.data) return;

		this._propertyEditorUIsSubscription = this._extensionRegistry
			?.extensionsOfType('propertyEditorUI')
			.subscribe((propertyEditorUIs) => {
				this._propertyEditorUIs = propertyEditorUIs;
				this._groupedPropertyEditorUIs = groupBy(propertyEditorUIs, 'group');
			});
	}

	private _handleClick(propertyEditorUI: ManifestPropertyEditorUI) {
		this._select(propertyEditorUI.alias);
	}

	private _select(alias: string) {
		this._selection = [alias];
	}

	private _handleFilterInput(event: UUIInputEvent) {
		let query = (event.target.value as string) || '';
		query = query.toLowerCase();

		const result = !query
			? this._propertyEditorUIs
			: this._propertyEditorUIs.filter((propertyEditorUI) => {
					return (
						propertyEditorUI.name.toLowerCase().includes(query) || propertyEditorUI.alias.toLowerCase().includes(query)
					);
			  });

		this._groupedPropertyEditorUIs = groupBy(result, 'group');
	}

	private _close() {
		this.modalHandler?.close();
	}

	private _submit() {
		this.modalHandler?.close({ selection: this._selection });
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._propertyEditorUIsSubscription?.unsubscribe();
	}

	render() {
		return html`
			<umb-editor-entity-layout headline="Select Property Editor UI">
				<uui-box> ${this._renderFilter()} ${this._renderGrid()} </uui-box>
				<div slot="actions">
					<uui-button label="Close" @click=${this._close}></uui-button>
					<uui-button label="${this._submitLabel}" look="primary" color="positive" @click=${this._submit}></uui-button>
				</div>
			</umb-editor-entity-layout>
		`;
	}

	private _renderFilter() {
		return html` <uui-input
			id="filter"
			@input="${this._handleFilterInput}"
			placeholder="Type to filter..."
			label="Type to filter icons">
			<uui-icon name="search" slot="prepend" id="filter-icon"></uui-icon>
		</uui-input>`;
	}

	private _renderGrid() {
		return html` ${Object.entries(this._groupedPropertyEditorUIs).map(
			([key, value]) =>
				html` <h4>${key}</h4>
					${this._renderGroupItems(value)}`
		)}`;
	}

	private _renderGroupItems(groupItems: Array<ManifestPropertyEditorUI>) {
		return html` <ul id="item-grid">
			${repeat(
				groupItems,
				(propertyEditorUI) => propertyEditorUI.alias,
				(propertyEditorUI) => html` <li class="item" ?selected=${this._selection.includes(propertyEditorUI.alias)}>
					<button type="button" @click="${() => this._handleClick(propertyEditorUI)}">
						<uui-icon name="${propertyEditorUI.meta.icon}" class="icon"></uui-icon>
						${propertyEditorUI.name}
					</button>
				</li>`
			)}
		</ul>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-modal-layout-property-editor-ui-picker': UmbModalLayoutPropertyEditorUIPickerElement;
	}
}
