import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UmbWorkspaceMediaContext } from './workspace-media.context';
import type { ManifestWorkspaceAction, ManifestWorkspaceView, ManifestWorkspaceViewCollection } from '@umbraco-cms/models';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '@umbraco-cms/context-api';

import '../shared/workspace-content/workspace-content.element';

@customElement('umb-workspace-media')
export class UmbWorkspaceMediaElement extends UmbContextConsumerMixin(UmbContextProviderMixin(LitElement)) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}
		`,
	];

	private _entityKey!: string;
	@property()
	public get entityKey(): string {
		return this._entityKey;
	}
	public set entityKey(value: string) {
		this._entityKey = value;
		this._provideWorkspace();
	}

	private _workspaceContext?:UmbWorkspaceMediaContext;


	constructor() {
		super();

		// TODO: consider if registering extensions should happen initially or else where, to enable unregister of extensions.
		this._registerWorkspaceViews();
	}

	connectedCallback(): void {
		super.connectedCallback();
		// TODO: avoid this connection, our own approach on Lit-Controller could be handling this case.
		this._workspaceContext?.connectedCallback();
	}
	disconnectedCallback(): void {
		super.connectedCallback()
		// TODO: avoid this connection, our own approach on Lit-Controller could be handling this case.
		this._workspaceContext?.disconnectedCallback();
	}

	protected _provideWorkspace() {
		if(this._entityKey) {
			this._workspaceContext = new UmbWorkspaceMediaContext(this, this._entityKey);
			this.provideContext('umbWorkspaceContext', this._workspaceContext);
		}
	}

	private _registerWorkspaceViews() {
		const dashboards: Array<ManifestWorkspaceView | ManifestWorkspaceViewCollection | ManifestWorkspaceAction> = [
			{
				type: 'workspaceViewCollection',
				alias: 'Umb.WorkspaceView.Media.Collection',
				name: 'Media Workspace Collection View',
				weight: 300,
				meta: {
					workspaces: ['Umb.Workspace.Media'],
					label: 'Media',
					pathname: 'collection',
					icon: 'umb:grid',
					entityType: 'media',
					storeAlias: 'umbMediaStore'
				},
			},
			{
				type: 'workspaceView',
				alias: 'Umb.WorkspaceView.Media.Edit',
				name: 'Media Workspace Edit View',
				loader: () => import('../shared/workspace-content/views/edit/workspace-view-content-edit.element'),
				weight: 200,
				meta: {
					workspaces: ['Umb.Workspace.Media'],
					label: 'Media',
					pathname: 'media',
					icon: 'umb:picture',
				},
			},
			{
				type: 'workspaceView',
				alias: 'Umb.WorkspaceView.Media.Info',
				name: 'Media Workspace Info View',
				loader: () => import('../shared/workspace-content/views/info/workspace-view-content-info.element'),
				weight: 100,
				meta: {
					workspaces: ['Umb.Workspace.Media'],
					label: 'Info',
					pathname: 'info',
					icon: 'info',
				},
			},
			{
				type: 'workspaceAction',
				alias: 'Umb.WorkspaceAction.Media.Save',
				name: 'Save Media Workspace Action',
				loader: () => import('../shared/actions/save/workspace-action-node-save.element'),
				meta: {
					workspaces: ['Umb.Workspace.Media'],
					look: 'primary',
					color: 'positive'
				},
			}
		];

		dashboards.forEach((dashboard) => {
			if (umbExtensionsRegistry.isRegistered(dashboard.alias)) return;
			umbExtensionsRegistry.register(dashboard);
		});
	}

	render() {
		return html`<umb-workspace-content alias="Umb.Workspace.Media"></umb-workspace-content>`;
	}
}

export default UmbWorkspaceMediaElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-media': UmbWorkspaceMediaElement;
	}
}
