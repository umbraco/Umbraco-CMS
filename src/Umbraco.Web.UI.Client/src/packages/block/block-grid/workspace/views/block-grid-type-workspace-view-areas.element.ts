import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-block-grid-type-workspace-view-areas')
export class UmbBlockTypeGridWorkspaceViewAreasElement extends UmbLitElement implements UmbWorkspaceViewElement {
	render() {
		return html`
			<uui-box headline="Areas">
				<umb-property
					label=${this.localize.term('blockEditor_areasLayoutColumns')}
					alias="areasLayoutColumns"
					property-editor-ui-alias="Umb.PropertyEditorUi.Number"></umb-property>
				<umb-property
					label=${this.localize.term('blockEditor_areasConfigurations')}
					alias="areas"
					property-editor-ui-alias="Umb.PropertyEditorUi.BlockGridAreas"></umb-property>
			</uui-box>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: grid;
				grid-template-columns: repeat(auto-fill, minmax(600px, 1fr));
				gap: var(--uui-size-layout-1);
				margin: var(--uui-size-layout-1);
				padding-bottom: var(--uui-size-layout-1); // To enforce some distance to the bottom of the scroll-container.
			}
		`,
	];
}

export default UmbBlockTypeGridWorkspaceViewAreasElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-grid-type-workspace-view-areas': UmbBlockTypeGridWorkspaceViewAreasElement;
	}
}
