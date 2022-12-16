import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import type { UmbWorkspaceContext } from '../shared/workspace/workspace.context';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import type { ManifestWorkspaceView } from '@umbraco-cms/models';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '@umbraco-cms/context-api';
import '../shared/workspace-content/workspace-content.element';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';

@customElement('umb-workspace-document')
export class UmbWorkspaceDocumentElement extends UmbObserverMixin(UmbContextConsumerMixin(UmbContextProviderMixin(LitElement))) {
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

	@property()
	entityKey!: string;

	private _workspaceContext!:UmbWorkspaceContext;

	constructor() {
		super();

		this.consumeContext('umbWorkspaceContext', (context) => {
			this._workspaceContext = context;
			this._observeWorkspace();
		})

		// TODO: consider if registering extensions should happen initially or else where, to enable unregister of extensions.
		this._registerWorkspaceViews();
	}
	private async _observeWorkspace() {
		if (!this._workspaceContext) return;

		this.observe(this._workspaceContext?.data, (workspaceData) => {
			this.entityKey = workspaceData.entityKey;
		});
	}

	private _registerWorkspaceViews() {
		const dashboards: Array<ManifestWorkspaceView> = [
			{
				type: 'workspaceView',
				alias: 'Umb.WorkspaceView.Document.Edit',
				name: 'Document Workspace Edit View',
				loader: () => import('../shared/workspace-content/views/edit/workspace-view-content-edit.element'),
				weight: 200,
				meta: {
					workspaces: ['Umb.Workspace.Document'],
					label: 'Info',
					pathname: 'content',
					icon: 'document',
				},
			},
			{
				type: 'workspaceView',
				alias: 'Umb.WorkspaceView.Document.Info',
				name: 'Document Workspace Info View',
				loader: () => import('../shared/workspace-content/views/info/workspace-view-content-info.element'),
				weight: 100,
				meta: {
					workspaces: ['Umb.Workspace.Document'],
					label: 'Info',
					pathname: 'info',
					icon: 'info',
				},
			},
		];

		dashboards.forEach((dashboard) => {
			if (umbExtensionsRegistry.isRegistered(dashboard.alias)) return;
			umbExtensionsRegistry.register(dashboard);
		});
	}

	render() {
		return html`<umb-workspace-content store-alias='umbDocumentStore' .entityKey=${this._workspaceContext.entityKey} alias="Umb.Workspace.Document"></umb-workspace-content>`;
	}
}

export default UmbWorkspaceDocumentElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-document': UmbWorkspaceDocumentElement;
	}
}
