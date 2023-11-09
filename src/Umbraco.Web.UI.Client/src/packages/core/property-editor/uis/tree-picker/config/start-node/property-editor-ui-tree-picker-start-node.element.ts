import { StartNode, UmbInputContentTypeElement } from '@umbraco-cms/backoffice/content-type';
import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-property-editor-ui-tree-picker-start-node
 */
@customElement('umb-property-editor-ui-tree-picker-start-node')
export class UmbPropertyEditorUITreePickerStartNodeElement extends UmbLitElement {
	@property({ type: Object })
	value?: StartNode;

	#onChange(event: CustomEvent) {
		const target = event.target as UmbInputContentTypeElement;

		this.value = {
			type: target.type,
			id: target.nodeId,
			query: target.dynamicPath,
		};

		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<umb-input-content-type
			@change="${this.#onChange}"
			.type=${this.value?.type}></umb-input-content-type>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUITreePickerStartNodeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tree-picker-start-node': UmbPropertyEditorUITreePickerStartNodeElement;
	}
}
