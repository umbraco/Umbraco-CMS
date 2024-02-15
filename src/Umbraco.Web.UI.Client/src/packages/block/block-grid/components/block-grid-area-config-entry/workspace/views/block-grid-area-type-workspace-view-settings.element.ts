import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-block-grid-area-type-workspace-view')
export class UmbBlockGridAreaTypeWorkspaceViewSettingsElement extends UmbLitElement implements UmbWorkspaceViewElement {
	// TODO: Localization:
	// TODO: Validation to prevent spaces and wierd characters in alias:
	// TODO: Add create button label field:
	// TODO: Add validation field:
	// TODO: Add validation permission field:
	render() {
		return html`
			<uui-box headline=${'Identification'}>
				<umb-property
					label=${this.localize.term('general_alias')}
					alias="alias"
					property-editor-ui-alias="Umb.PropertyEditorUi.TextBox"></umb-property>

				<umb-property
					label=${'Temp column span:'}
					alias="columnSpan"
					property-editor-ui-alias="Umb.PropertyEditorUi.Number"></umb-property>
			</uui-box>
			<uui-box headline=${'Validation'}> TO BE DONE. </uui-box>
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

			#showOptions {
				display: flex;
				height: 100px;
			}
			#showOptions uui-button {
				flex: 1;
			}
		`,
	];
}

export default UmbBlockGridAreaTypeWorkspaceViewSettingsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-grid-area-type-workspace-view-settings': UmbBlockGridAreaTypeWorkspaceViewSettingsElement;
	}
}
