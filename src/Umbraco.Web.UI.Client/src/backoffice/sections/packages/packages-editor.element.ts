import '../../editors/shared/editor-entity/editor-entity.element';

import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

import { UmbContextConsumerMixin } from '../../../core/context';
import { UmbExtensionRegistry } from '../../../core/extension';

@customElement('umb-packages-editor')
export class UmbPackagesEditor extends UmbContextConsumerMixin(LitElement) {
	private umbExtensionRegistry?: UmbExtensionRegistry;

	constructor() {
		super();

		this.consumeContext('umbExtensionRegistry', (umbExtensionRegistry: UmbExtensionRegistry) => {
			this.umbExtensionRegistry = umbExtensionRegistry;
			this._registerViews();
		});
	}

	private _registerViews() {
		this.umbExtensionRegistry?.register({
			alias: 'Umb.Editor.Packages.Overview',
			name: 'Packages',
			type: 'editorView',
			elementName: 'umb-packages-overview',
			loader: () => import('./packages-overview.element'),
			meta: {
				icon: 'document',
				pathname: 'repo',
				editors: ['Umb.Editor.Packages'],
				weight: 10,
			},
		});

		this.umbExtensionRegistry?.register({
			alias: 'Umb.Editor.Packages.Installed',
			name: 'Installed',
			type: 'editorView',
			elementName: 'umb-packages-installed',
			loader: () => import('./packages-installed.element'),
			meta: {
				icon: 'document',
				pathname: 'installed',
				editors: ['Umb.Editor.Packages'],
				weight: 0,
			},
		});
	}

	render() {
		return html`
			<uui-icon-registry-essential>
				<umb-section-layout>
					<umb-section-main>
						<umb-editor-entity alias="Umb.Editor.Packages">
							<h1 slot="name">Packages</h1>
						</umb-editor-entity>
					</umb-section-main>
				</umb-section-layout>
			</uui-icon-registry-essential>
		`;
	}
}

export default UmbPackagesEditor;

declare global {
	interface HTMLElementTagNameMap {
		'umb-packages-editor': UmbPackagesEditor;
	}
}
