import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';

@customElement('umb-editor-view-node-edit')
export class UmbEditorViewNodeEdit extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			hr {
				border: 0;
				/* TODO: Use correct color property */
				border-top: 1px solid #e7e7e7;
			}
		`,
	];

	@property()
	node: any;

	render() {
		return html`
			<uui-box>
				<!-- TODO: Make sure map get data from data object?, parse on property object. -->
				${this.node?.properties.map(
					(property: any) => html`
						<umb-node-property .property=${property} .value=${property.tempValue}> </umb-node-property>
						<hr />
					`
				)}
			</uui-box>
		`;
	}
}

export default UmbEditorViewNodeEdit;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-view-node-edit': UmbEditorViewNodeEdit;
	}
}
