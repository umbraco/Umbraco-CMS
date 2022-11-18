import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import type { ManifestEditorView, ManifestWithLoader } from '@umbraco-cms/models';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';

import '../shared/editor-content/editor-node.element';

@customElement('umb-editor-media')
export class UmbEditorMediaElement extends LitElement {
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
	}

	private _registerEditorViews() {
		const dashboards: Array<ManifestWithLoader<ManifestEditorView>> = [
			{
				type: 'editorView',
				alias: 'Umb.EditorView.Media.Edit',
				name: 'Media Editor Edit View',
				loader: () => import('../shared/editor-content/views/edit/editor-view-node-edit.element'),
				weight: 200,
				meta: {
					editors: ['Umb.Editor.Media'],
					label: 'Media',
					pathname: 'media',
					icon: 'umb:picture',
				},
			},
			{
				type: 'editorView',
				alias: 'Umb.EditorView.Media.Info',
				name: 'Media Editor Info View',
				loader: () => import('../shared/editor-content/views/info/editor-view-node-info.element'),
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
		return html`<umb-editor-node .entityKey=${this.entityKey} alias="Umb.Editor.Media"></umb-editor-node>`;
	}
}

export default UmbEditorMediaElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-media': UmbEditorMediaElement;
	}
}
