import { StartNode } from '@umbraco-cms/backoffice/components';
import { UmbInputDocumentElement } from '@umbraco-cms/backoffice/document';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
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
	private _list: Array<Option> = [];

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		const listData = config.getValueByAlias('items') as Array<Option>;
		if (!listData.length) return;

		this._list = listData.map((item): Option => {
			const option: Option = { value: item.value, name: item.name, selected: false };
			if (item.value === this.value.type) {
				option.selected = true;
			}
			return option;
		});
	}

	#onChangeType(event: UUISelectEvent) {
		this.value = { ...this.value, type: event.target.value as StartNode['type'] };
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}
	#onChangeId(event: CustomEvent) {
		this.value = {
			...this.value,
			id: (event.target as UmbInputDocumentElement | UmbInputMediaElement).selectedIds.join() as StartNode['id'],
		};
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<umb-input-dropdown-list
				@change="${this.#onChangeType}"
				.options="${this._list}"></umb-input-dropdown-list>
			${this.renderOption()}`;
	}

	renderOption() {
		switch (this.value.type) {
			case 'content':
				return this.renderContentOption();
			case 'media':
				return this.renderMediaOption();
			default:
				return '';
		}
	}

	renderContentOption() {
		return html`<umb-input-document max="1" @change=${this.#onChangeId}></umb-input-document>`;
	}

	renderMediaOption() {
		//TODO: Media Type picker
		return html`<umb-input-media-types>umb-input-media-types</umb-input-media-types>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUITreePickerStartNodeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tree-picker-start-node': UmbPropertyEditorUITreePickerStartNodeElement;
	}
}
