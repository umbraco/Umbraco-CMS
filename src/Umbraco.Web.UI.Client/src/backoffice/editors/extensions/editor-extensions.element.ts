import { html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';

import { UmbContextConsumerMixin } from '../../../core/context';
import { UmbExtensionRegistry } from '../../../core/extension';
import { isManifestElementType } from '../../../core/extension/is-extension.function';

import type { ManifestTypes } from '../../../core/models';
import { UmbObserverMixin } from '../../../core/observer';

import '../shared/editor-entity-layout/editor-entity-layout.element';

@customElement('umb-editor-extensions')
export class UmbEditorExtensionsElement extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	@state()
	private _extensions: Array<ManifestTypes> = [];

	private _extensionRegistry?: UmbExtensionRegistry;

	constructor() {
		super();

		this.consumeContext('umbExtensionRegistry', (_instance: UmbExtensionRegistry) => {
			this._extensionRegistry = _instance;
			this._observeExtensions();
		});
	}

	private _observeExtensions() {
		if (!this._extensionRegistry) return;

		this.observe<Array<ManifestTypes>>(this._extensionRegistry.extensions, (extensions) => {
			this._extensions = [...extensions]; // TODO: Though, this is a shallow clone, wouldn't we either do a deep clone or no clone at all?
		});
	}

	render() {
		return html`
			<umb-editor-entity-layout headline="Extensions" alias="Umb.Editor.Extensions">
				<uui-box headline="Extensions">
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
