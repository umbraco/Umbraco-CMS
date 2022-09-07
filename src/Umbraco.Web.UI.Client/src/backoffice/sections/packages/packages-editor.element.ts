import '../../editors/shared/editor-entity/editor-entity.element';

import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-packages-editor')
export class UmbPackagesEditor extends LitElement {
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
