import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UmbUserStore, UMB_USER_STORE_CONTEXT_TOKEN } from '../../../users/repository/user.store';
import type { IRoute, IRoutingInfo } from '@umbraco-cms/router';
import { umbExtensionsRegistry, createExtensionElement } from '@umbraco-cms/extensions-api';

import './list-view-layouts/table/workspace-view-users-table.element';
import './list-view-layouts/grid/workspace-view-users-grid.element';
import './workspace-view-users-selection.element';

import type { ManifestWorkspace, UserDetails } from '@umbraco-cms/models';
import { UmbLitElement } from '@umbraco-cms/element';
import { DeepState } from '@umbraco-cms/observable-api';

@customElement('umb-section-view-users')
export class UmbSectionViewUsersElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				height: 100%;
			}
		`,
	];

	@state()
	private _routes: IRoute[] = [];

	private _workspaces: Array<ManifestWorkspace> = [];

	// TODO: This must be turned into context api: Maybe its a Collection View (SectionView Collection View)?
	private _userStore?: UmbUserStore;

	#selection = new DeepState(<Array<string>>[]);
	public readonly selection = this.#selection.asObservable();

	#users = new DeepState(<Array<UserDetails>>[]);
	public readonly users = this.#users.asObservable();

	#search = new DeepState('');
	public readonly search = this.#search.asObservable();

	constructor() {
		super();

		this.consumeContext<UmbUserStore>(UMB_USER_STORE_CONTEXT_TOKEN, (_instance) => {
			this._userStore = _instance;
			this._observeUsers();
		});

		// TODO: consider this context name, is it to broad?
		// TODO: Stop using it self as a context api.
		this.provideContext('umbUsersContext', this);

		this.observe(umbExtensionsRegistry?.extensionsOfType('workspace'), (workspaceExtensions) => {
			this._workspaces = workspaceExtensions;
			this._createRoutes();
		});
	}

	private _createRoutes() {
		const routes: any[] = [
			{
				path: 'overview',
				component: () => import('./workspace-view-users-overview.element'),
			},
		];

		// TODO: find a way to make this reuseable across:
		this._workspaces?.map((workspace: ManifestWorkspace) => {
			routes.push({
				path: `${workspace.meta.entityType}/:key`,
				component: () => createExtensionElement(workspace),
				setup: (component: Promise<HTMLElement>, info: IRoutingInfo) => {
					component.then((el: HTMLElement) => {
						(el as any).entityKey = info.match.params.key;
					});
				},
			});
			routes.push({
				path: workspace.meta.entityType,
				component: () => createExtensionElement(workspace),
			});
		});

		routes.push({
			path: '**',
			redirectTo: 'overview',
		});
		this._routes = routes;
	}

	private _observeUsers() {
		if (!this._userStore) return;

		if (this.#search.getValue()) {
			this.observe(this._userStore.getByName(this.#search.getValue()), (users) => this.#users.next(users));
		} else {
			this.observe(this._userStore.getAll(), (users) => this.#users.next(users));
		}
	}

	public setSearch(value?: string) {
		this.#search.next(value || '');
		this._observeUsers();
		this.requestUpdate('search');
	}

	public setSelection(value: Array<string>) {
		if (!value) return;
		this.#selection.next(value);
		this.requestUpdate('selection');
	}

	public select(key: string) {
		const oldSelection = this.#selection.getValue();
		if (oldSelection.indexOf(key) !== -1) return;

		this.#selection.next([...oldSelection, key]);
		this.requestUpdate('selection');
	}

	public deselect(key: string) {
		const selection = this.#selection.getValue();
		this.#selection.next(selection.filter((k) => k !== key));
		this.requestUpdate('selection');
	}

	render() {
		return html`<umb-router-slot .routes=${this._routes}></umb-router-slot>`;
	}
}

export default UmbSectionViewUsersElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-view-users': UmbSectionViewUsersElement;
	}
}
