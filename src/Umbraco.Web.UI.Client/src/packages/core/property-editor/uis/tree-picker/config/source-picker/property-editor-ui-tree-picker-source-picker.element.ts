import { StartNode, UmbInputTreePickerSourceElement } from '@umbraco-cms/backoffice/components';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-property-editor-ui-tree-picker-source-picker
 */
@customElement('umb-property-editor-ui-tree-picker-source-picker')
export class UmbPropertyEditorUITreePickerSourcePickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property({ type: Object })
	value?: StartNode;

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	#onChange(event: CustomEvent) {
		const target = event.target as UmbInputTreePickerSourceElement;

		this.value = {
			type: target.type,
			id: target.nodeId,
			dynamicRoot: target.dynamicRoot,
		};

		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<umb-input-tree-picker-source
			@change=${this.#onChange}
			.type=${this.value?.type}
			.nodeId=${this.value?.id}></umb-input-tree-picker-source>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUITreePickerSourcePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tree-picker-source-picker': UmbPropertyEditorUITreePickerSourcePickerElement;
	}
}
