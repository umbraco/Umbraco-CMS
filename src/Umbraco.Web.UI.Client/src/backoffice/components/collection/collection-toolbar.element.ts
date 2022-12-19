import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import '../tooltip-menu.element';
import { TooltipMenuItem } from '../tooltip-menu.element';

@customElement('umb-collection-toolbar')
export class UmbCollectionToolbarElement extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				gap: var(--uui-size-3);
				width: 100%;
				padding: var(--uui-size-space-4) var(--uui-size-space-6);
				align-items: center;
				box-sizing: border-box;
			}

			uui-popover {
				width: min-content;
			}
			#search {
				width: 100%;
			}
		`,
	];

	@property()
	public viewTypes: Array<TooltipMenuItem> = [
		{
			label: 'List',
			icon: 'umb:list',
			action: () => {
				this._currentViewType = this.viewTypes[0];
			},
		},
		{
			label: 'Grid',
			icon: 'umb:grid',
			action: () => {
				console.log('aweawd');
				this._currentViewType = this.viewTypes[1];
			},
		},
		{
			label: 'something else',
			icon: 'umb:folder',
			action: () => {
				this._currentViewType = this.viewTypes[2];
			},
		},
	];
	@property()
	public actions: Array<TooltipMenuItem> = [
		{
			label: 'File',
			icon: 'umb:document',
			action: () => {
				console.log('Create file');
			},
		},
		{
			label: 'Folder',
			icon: 'umb:folder',
			action: () => {
				console.log('create folder');
			},
		},
	];

	@property()
	public useSearch = true;

	@state()
	private _currentViewType = this.viewTypes[0];
	@state()
	private _search = '';
	@state()
	private _actionsOpen = false;
	@state()
	private _viewTypesOpen = false;

	private _toggleViewType() {
		const index = this.viewTypes.indexOf(this._currentViewType);
		this._currentViewType = this.viewTypes[(index + 1) % this.viewTypes.length];
	}

	private _updateSearch(e: InputEvent) {
		this._search = (e.target as HTMLInputElement).value;

		this.dispatchEvent(
			new CustomEvent('search', {
				detail: this._search,
			})
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

	private _renderViewTypeButton() {
		if (this.viewTypes.length < 2 || !this._currentViewType.icon) return nothing;

		if (this.viewTypes.length === 2) {
			return html`<uui-button @click=${this._toggleViewType} look="outline" compact>
				<uui-icon .name=${this._currentViewType.icon}></uui-icon>
			</uui-button>`;
		}
		if (this.viewTypes.length > 2) {
			return html`<uui-popover margin="8" .open=${this._viewTypesOpen} @close=${() => (this._viewTypesOpen = false)}>
				<uui-button @click=${() => (this._viewTypesOpen = !this._viewTypesOpen)} slot="trigger" look="outline" compact>
					<uui-icon .name=${this._currentViewType.icon}></uui-icon>
				</uui-button>
				<umb-tooltip-menu icon slot="popover" .items=${this.viewTypes}></umb-tooltip-menu>
			</uui-popover>`;
		}

		return nothing;
	}

	render() {
		return html`
			${this._renderCreateButton()}
			<uui-input id="search" @input=${this._updateSearch}></uui-input>
			${this._renderViewTypeButton()}
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-toolbar': UmbCollectionToolbarElement;
	}
}
