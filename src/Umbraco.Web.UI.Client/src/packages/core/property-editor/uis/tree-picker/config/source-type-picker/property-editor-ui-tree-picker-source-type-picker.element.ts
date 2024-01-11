import { UmbInputDocumentTypeElement } from '@umbraco-cms/backoffice/document-type';
import { UmbMediaTypeInputElement } from '@umbraco-cms/backoffice/media-type';
import { UmbMemberTypeInputElement } from '@umbraco-cms/backoffice/member-type';
import type { UmbTreePickerSource } from '@umbraco-cms/backoffice/components';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';

/**
 * @element umb-property-editor-ui-tree-picker-source-type-picker
 */
@customElement('umb-property-editor-ui-tree-picker-source-type-picker')
export class UmbPropertyEditorUITreePickerSourceTypePickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	#datasetContext?: typeof UMB_PROPERTY_DATASET_CONTEXT.TYPE;

	@property({ type: Array })
	value?: string[];

	@state()
	private sourceType: string = 'content';

	#initialized: boolean = false;

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, (datasetContext) => {
			this.#datasetContext = datasetContext;
			this._observeProperty();
		});
	}

	private async _observeProperty() {
		if (!this.#datasetContext) return;
		this.observe(
			await this.#datasetContext.propertyValueByAlias('startNode'),
			(value) => {
				const startNode = value as UmbTreePickerSource;
				if (startNode.type) {
					// If we had a sourceType before, we can see this as a change and not the initial value,
					// so let's reset the value, so we don't carry over content-types to the new source type.
					if (this.#initialized && this.sourceType !== startNode.type) {
						this.value = [];
					}

					this.sourceType = startNode.type;

					if (!this.#initialized) {
						this.#initialized = true;
					}
				}
			},
			'observeValue',
		);
	}

	#onChange(event: CustomEvent) {
		switch (this.sourceType) {
			case 'content':
				this.value = (<UmbInputDocumentTypeElement>event.target).selectedIds;
				break;
			case 'media':
				this.value = (<UmbMediaTypeInputElement>event.target).selectedIds;
				break;
			case 'member':
				this.value = (<UmbMemberTypeInputElement>event.target).selectedIds;
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
				return 'No source type found';
		}
	}

	#renderTypeContent() {
		return html`<umb-input-document-type
			@change=${this.#onChange}
			.selectedIds=${this.value || []}></umb-input-document-type>`;
	}

	#renderTypeMedia() {
		return html`<umb-media-type-input
			@change=${this.#onChange}
			.selectedIds=${this.value || []}></umb-media-type-input>`;
	}

	#renderTypeMember() {
		return html`<umb-input-member-type
			@change=${this.#onChange}
			.selectedIds=${this.value || []}></umb-input-member-type>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUITreePickerSourceTypePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tree-picker-source-type-picker': UmbPropertyEditorUITreePickerSourceTypePickerElement;
	}
}
