import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import type { ManifestWorkspaceView } from '@umbraco-cms/models';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import { UmbMediaStore } from 'src/core/stores/media/media.store';
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

	@property()
	entityKey!: string;

	constructor() {
		super();

		this._registerWorkspaceViews();

		this.consumeContext('umbMediaStore', (mediaStore: UmbMediaStore) => {
			this.provideContext('umbContentStore', mediaStore);
		});
	}

	private _registerWorkspaceViews() {
		const dashboards: Array<ManifestWorkspaceView> = [
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
		];

		dashboards.forEach((dashboard) => {
			if (umbExtensionsRegistry.isRegistered(dashboard.alias)) return;
			umbExtensionsRegistry.register(dashboard);
		});
	}

	render() {
		return html`<umb-workspace-content .entityKey=${this.entityKey} alias="Umb.Workspace.Media"></umb-workspace-content>`;
	}
}

export default UmbWorkspaceMediaElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-media': UmbWorkspaceMediaElement;
	}
}
