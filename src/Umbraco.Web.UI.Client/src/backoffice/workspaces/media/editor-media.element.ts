import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import type { ManifestWorkspaceView } from '@umbraco-cms/models';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import { UmbMediaStore } from 'src/core/stores/media/media.store';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '@umbraco-cms/context-api';

import '../shared/workspace-content/workspace-content.element';

@customElement('umb-editor-media')
export class UmbEditorMediaElement extends UmbContextConsumerMixin(UmbContextProviderMixin(LitElement)) {
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

		this._registerEditorViews();

		this.consumeContext('umbMediaStore', (mediaStore: UmbMediaStore) => {
			this.provideContext('umbContentStore', mediaStore);
		});
	}

	private _registerEditorViews() {
		const dashboards: Array<ManifestWorkspaceView> = [
			{
				type: 'workspaceView',
				alias: 'Umb.EditorView.Media.Edit',
				name: 'Media Editor Edit View',
				loader: () => import('../shared/workspace-content/views/edit/editor-view-content-edit.element'),
				weight: 200,
				meta: {
					editors: ['Umb.Editor.Media'],
					label: 'Media',
					pathname: 'media',
					icon: 'umb:picture',
				},
			},
			{
				type: 'workspaceView',
				alias: 'Umb.EditorView.Media.Info',
				name: 'Media Editor Info View',
				loader: () => import('../shared/workspace-content/views/info/editor-view-content-info.element'),
				weight: 100,
				meta: {
					editors: ['Umb.Editor.Media'],
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
		return html`<umb-workspace-content .entityKey=${this.entityKey} alias="Umb.Editor.Media"></umb-workspace-content>`;
	}
}

export default UmbEditorMediaElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-media': UmbEditorMediaElement;
	}
}
