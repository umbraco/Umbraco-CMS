import { StartNode, UmbInputContentTypeElement } from '@umbraco-cms/backoffice/content-type';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorConfigCollection } from 'src/packages/core/property/property-editor';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-property-editor-ui-tree-picker-start-node
 */
@customElement('umb-property-editor-ui-tree-picker-start-node')
export class UmbPropertyEditorUITreePickerStartNodeElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property({ type: Object })
	value?: StartNode;

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

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
