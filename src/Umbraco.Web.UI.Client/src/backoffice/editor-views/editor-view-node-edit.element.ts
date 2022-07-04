import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { DocumentNode, NodeProperty } from '../../mocks/data/content.data';

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

	@property({ type: Object })
	node?: DocumentNode;

	render() {
		return html`
			<uui-box>
				${this.node?.properties.map(
					(property: NodeProperty) => html`
						<umb-node-property
							.property=${property}
							.value=${this.node?.data.find((data) => data.alias === property.alias)?.value}></umb-node-property>
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
