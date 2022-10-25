import { html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { isManifestElementType } from '@umbraco-cms/extensions-api';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import type { ManifestTypes } from '@umbraco-cms/models';

import '../shared/editor-entity-layout/editor-entity-layout.element';

@customElement('umb-editor-extensions')
export class UmbEditorExtensionsElement extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	@state()
	private _extensions: Array<ManifestTypes> = [];

	connectedCallback(): void {
		super.connectedCallback();
		this._observeExtensions();
	}

	private _observeExtensions() {
		this.observe<Array<ManifestTypes>>(umbExtensionsRegistry.extensions, (extensions) => {
			this._extensions = [...extensions];
		});
	}

	render() {
		return html`
			<umb-editor-entity-layout headline="Extensions" alias="Umb.Editor.Extensions">
				<uui-box>
					<p>List of currently loaded extensions</p>
					<uui-table>
						<uui-table-head>
							<uui-table-head-cell>Type</uui-table-head-cell>
							<uui-table-head-cell>Name</uui-table-head-cell>
							<uui-table-head-cell>Alias</uui-table-head-cell>
						</uui-table-head>

						${this._extensions.map(
							(extension) => html`
								<uui-table-row>
									<uui-table-cell>${extension.type}</uui-table-cell>
									<uui-table-cell>
										${isManifestElementType(extension) ? extension.name : 'Custom extension'}
									</uui-table-cell>
									<uui-table-cell>${extension.alias}</uui-table-cell>
								</uui-table-row>
							`
						)}
					</uui-table>
				</uui-box>
			</umb-editor-entity-layout>
		`;
	}
}

export default UmbEditorExtensionsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-extensions': UmbEditorExtensionsElement;
	}
}
