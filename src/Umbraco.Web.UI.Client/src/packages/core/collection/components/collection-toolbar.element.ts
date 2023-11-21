import type { TooltipMenuItem } from '../../components/tooltip-menu/index.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, nothing, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { ManifestCollectionView } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-collection-toolbar')
export class UmbCollectionToolbarElement extends UmbLitElement {
	@property()
	public actions: Array<TooltipMenuItem> = [
		{
			label: 'File',
			icon: 'icon-document',
			action: () => {
				console.log('Create file');
			},
		},
		{
			label: 'Folder',
			icon: 'icon-folder',
			action: () => {
				console.log('create folder');
			},
		},
	];

	@property()
	public useSearch = true;

	@state()
	private _layouts: Array<ManifestCollectionView> = [];

	@state()
	private _currentLayout?: ManifestCollectionView;

	@state()
	private _search = '';
	@state()
	private _actionsOpen = false;
	@state()
	private _viewTypesOpen = false;

	constructor() {
		super();
	}

	private _changeLayout(path: string) {
		history.pushState(null, '', 'section/media/dashboard/media-management/' + path);
	}

	private _updateSearch(e: InputEvent) {
		this._search = (e.target as HTMLInputElement).value;

		this.dispatchEvent(
			new CustomEvent('search', {
				detail: this._search,
			}),
		);
	}

	private _renderCreateButton() {
		if (this.actions.length === 0) return nothing;

		if (this.actions.length === 1) {
			return html`<uui-button @click=${() => this.actions[0].action()} look="outline">Create</uui-button>`;
		}
		if (this.actions.length > 1) {
			return html`<uui-popover margin="8" .open=${this._actionsOpen} @close=${() => (this._actionsOpen = false)}>
				<uui-button @click=${() => (this._actionsOpen = !this._actionsOpen)} slot="trigger" look="outline">
					Create
				</uui-button>
				<umb-tooltip-menu slot="popover" .items=${this.actions}></umb-tooltip-menu>
			</uui-popover>`;
		}

		return nothing;
	}

	render() {
		return html`
			${this._renderCreateButton()}
			<umb-extension-slot type="collectionAction"></umb-extension-slot>
			<uui-input id="search" @input=${this._updateSearch}></uui-input>
			<umb-collection-view-bundle></umb-collection-view-bundle>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				gap: var(--uui-size-space-5);
				width: 100%;
			}

			uui-popover {
				width: min-content;
			}
			#search {
				width: 100%;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-toolbar': UmbCollectionToolbarElement;
	}
}
