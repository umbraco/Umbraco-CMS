import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, nothing, PropertyValueMap } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { DeepState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

// TODO: Refactor this, its not a service and the data should be handled by a context api.
@customElement('umb-tree-context-menu-page-service')
export class UmbTreeContextMenuPageServiceElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	@property({ type: Object })
	public actionEntity: any = { key: '', name: '' };

	#entity = new DeepState({ key: '', name: '' } as any);
	public readonly entity = this.#entity.asObservable();

	@state()
	private _pages: Array<HTMLElement> = [];

	connectedCallback() {
		super.connectedCallback();
		this.provideContext(UMB_TREE_CONTEXT_MENU_PAGE_SERVICE_CONTEXT_TOKEN, this);
		this.openFreshPage('umb-tree-context-menu-page-action-list');
	}

	protected updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.updated(_changedProperties);

		if (_changedProperties.has('actionEntity')) {
			this.#entity.next(this.actionEntity);
			//TODO: Move back to first page
			this.openFreshPage('umb-tree-context-menu-page-action-list');
		}
	}

	public openPage(elementName: string) {
		const element = document.createElement(elementName) as any;
		this._pages.push(element);
		this.requestUpdate('_pages');
	}

	public openFreshPage(elementName: string) {
		this._pages = [];
		this.openPage(elementName);
	}

	public closeTopPage() {
		this._pages.pop();
		this.requestUpdate('_pages');
	}

	private _renderTopPage() {
		if (this._pages.length === 0) {
			return nothing;
		}

		return this._pages[this._pages.length - 1];
	}

	render() {
		return this._renderTopPage();
	}
}

export const UMB_TREE_CONTEXT_MENU_PAGE_SERVICE_CONTEXT_TOKEN =
	new UmbContextToken<UmbTreeContextMenuPageServiceElement>('UmbTreeContextMenuService');

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-context-menu-page-service': UmbTreeContextMenuPageServiceElement;
	}
}
