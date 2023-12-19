import { UMB_DATA_TYPE_WORKSPACE_CONTEXT } from '../../../../../data-type/workspace/data-type-workspace.context.js';
import { UmbDocumentTypeInputElement } from '../../../../../../documents/document-types/components/document-type-input/document-type-input.element.js';
import { UmbMediaTypeInputElement } from '../../../../../../media/media-types/components/media-type-input/media-type-input.element.js';
import { StartNode } from '@umbraco-cms/backoffice/components';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-property-editor-ui-tree-picker-filter')
export class UmbPropertyEditorUITreePickerFilterElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	constructor() {
		super();

		this.consumeContext(UMB_DATA_TYPE_WORKSPACE_CONTEXT, (instance) => {
			const workspace = instance;
			this.observe(workspace.data, (data) => {
				const property = data?.values.find((setting) => setting.alias === 'startNode');
				if (property) {
					const startNode = property.value as StartNode;
					if (startNode.type) {
						this.sourceType = startNode.type;
					}
				}
			});
		});
	}

	@property({ type: Array })
	value?: string[];

	@property({ attribute: false })
	sourceType: string = 'content';

	#onChange(event: CustomEvent) {
		switch (this.sourceType) {
			case 'content':
				this.value = (<UmbDocumentTypeInputElement>event.target).selectedIds;
				break;
			case 'media':
				this.value = (<UmbMediaTypeInputElement>event.target).selectedIds;
				break;
			default:
				break;
		}

		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return this.#renderType();
	}

	#renderType() {
		switch (this.sourceType) {
			case 'content':
				return this.#renderTypeContent();
			case 'media':
				return this.#renderTypeMedia();
			case 'member':
				return this.#renderTypeMember();
			default:
				// TODO: Could we make this message to be friendlier? [LK]
				return 'No source type found';
		}
	}

	#renderTypeContent() {
		return html`<umb-document-type-input
			@change=${this.#onChange}
			.selectedIds=${this.value || []}></umb-document-type-input>`;
	}

	#renderTypeMedia() {
		return html`<umb-media-type-input
			@change=${this.#onChange}
			.selectedIds=${this.value || []}></umb-media-type-input>`;
	}

	#renderTypeMember() {
		return html`<umb-member-type-input
			@change=${this.#onChange}
			.selectedIds=${this.value || []}></umb-member-type-input>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUITreePickerFilterElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tree-picker-filter': UmbPropertyEditorUITreePickerFilterElement;
	}
}
