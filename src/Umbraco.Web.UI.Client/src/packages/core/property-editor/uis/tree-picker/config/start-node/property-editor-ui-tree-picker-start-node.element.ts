import { StartNode, UmbInputContentTypeElement } from '@umbraco-cms/backoffice/content-type';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
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
		const target = event.target as UmbInputContentTypeElement;

		this.value = {
			type: target.startNodeType,
			id: target.startNodeId,
		};

		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<umb-input-content-type
			@change="${this.#onChange}"
			.options="${this._options}"
			.startNodeType=${this.value.type}></umb-input-content-type>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUITreePickerStartNodeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tree-picker-start-node': UmbPropertyEditorUITreePickerStartNodeElement;
	}
}
