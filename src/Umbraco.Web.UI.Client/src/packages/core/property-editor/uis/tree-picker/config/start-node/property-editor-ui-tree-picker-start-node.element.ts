import { StartNode, UmbInputStartNodeElement } from '@umbraco-cms/backoffice/components';
import { UmbInputDocumentElement } from '@umbraco-cms/backoffice/document';
import { html, customElement, property, state, css } from '@umbraco-cms/backoffice/external/lit';
import { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbInputMediaElement } from '@umbraco-cms/backoffice/media';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-property-editor-ui-tree-picker-start-node
 */
@customElement('umb-property-editor-ui-tree-picker-start-node')
export class UmbPropertyEditorUITreePickerStartNodeElement extends UmbLitElement {
	@property({ type: Object })
	value: StartNode = {
		type: 'content',
	};

	@state()
	private _options: Array<Option> = [];

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		const listData = config.getValueByAlias('items') as Array<Option>;
		if (!listData.length) return;

		this._options = listData.map((item): Option => {
			const option: Option = { value: item.value, name: item.name, selected: false };
			if (item.value === this.value.type) {
				option.selected = true;
			}
			return option;
		});
	}

	#onChange(event: CustomEvent) {
		//this.value = { ...this.value, ...((event.target as UmbInputStartNodeElement).startNode as StartNode) };
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<umb-input-start-node
			@change="${this.#onChange}"
			.options="${this._options}"
			.startNodeType=${this.value.type}></umb-input-start-node>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUITreePickerStartNodeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tree-picker-start-node': UmbPropertyEditorUITreePickerStartNodeElement;
	}
}
